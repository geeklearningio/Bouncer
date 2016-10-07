-- ===============================================================
-- Author:		Adrien Siffermann
-- Create date: 06/10/2016
-- Description:	Find all rights of a given scope and its children
-- for a given principal.
-- ===============================================================
CREATE function [Authorizations].[GetRightsForScopeAndChildren]
(	
	@scopeName nvarchar(150), 
	@principalId uniqueidentifier
)
returns table 
as
return 
(
	with ChildScopes_CTE (ScopeId, ScopeName, InheritedRights, ExplicitRights, ScopeHierarchy)
	as
	(
		select
			s.[Id] as ScopeId,
			s.[Name] as ScopeName,
			Authorizations.GetInheritedRightsForScope(@scopeName, @principalId) as InheritedRights,
			dbo.RemoveDuplicates(Rights.Names) as ExplicitRights,
			cast(s.[Name] as nvarchar(max)) as ScopeHierarchy
		from Authorizations.Scope s 
		cross apply (
			select stuff(replace((select '#!' + ltrim(rtrim(ri.[Name])) as 'data()'
			from Authorizations.[Authorization] a
			inner join Authorizations.[Role] ro on ro.Id = a.RoleId
			inner join Authorizations.[RoleRight] rr on rr.RoleId = ro.Id
			inner join Authorizations.[Right] ri on ri.Id = rr.RightId
			where a.ScopeId = s.Id
			and a.PrincipalId = @principalId
			for xml path('')),' #!',','), 1, 2, '') as Names
		) Rights
		where s.Name = @scopeName

		union all

		select
			s.[Id] as ScopeId,
			s.[Name] as ScopeName,
			dbo.RemoveDuplicates(case 
				when cte.InheritedRights is null then Rights.Names
				when Rights.Names is null then cte.InheritedRights
				else cte.InheritedRights + ',' + Rights.Names
			end) as InheritedRights,
			dbo.RemoveDuplicates(Rights.Names) as ExplicitRights,
			cast(cte.ScopeHierarchy + '/' + s.[Name] as nvarchar(max)) as ScopeHierarchy
		from Authorizations.ScopeHierarchy sh
		inner join ChildScopes_CTE cte on cte.ScopeId = sh.ParentId
		inner join Authorizations.Scope s on s.Id = sh.ChildId
		cross apply (
			select stuff(replace((select '#!' + ltrim(rtrim(ri.[Name])) as 'data()'
			from Authorizations.[Authorization] a
			inner join Authorizations.[Role] ro on ro.Id = a.RoleId
			inner join Authorizations.[RoleRight] rr on rr.RoleId = ro.Id
			inner join Authorizations.[Right] ri on ri.Id = rr.RightId
			where a.ScopeId = s.Id
			and a.PrincipalId = @principalId
			for xml path('')),' #!',','), 1, 2, '') as Names
		) Rights
	)
	select *
	from ChildScopes_CTE
	where InheritedRights is not null
)