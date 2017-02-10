-- ======================================================================
-- Author:		Adrien Siffermann
-- Create date: 06/10/2016
-- Description:	Find all inherited rights from parents of a given scope
-- for a given principal.
-- ======================================================================
create function [Authorizations].[GetInheritedRightsForScope]
(
	@scopeName nvarchar(150), 
	@principalId uniqueidentifier
)
returns nvarchar(max)
as
begin
	declare @inheritedRights nvarchar(max);

	with ParentScopes_CTE 
	as
	(
		select
			s.[Id] as ScopeId,
			dbo.RemoveDuplicates(Rights.Names) as ExplicitRights
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
		where s.[Name] = @scopeName

		union all

		select
			sh.ParentId as ScopeId,
			dbo.RemoveDuplicates(Rights.Names) as ExplicitRights
		from Authorizations.ScopeHierarchy sh
		inner join ParentScopes_CTE cte on cte.ScopeId = sh.ChildId
		cross apply (
			select stuff(replace((select '#!' + ltrim(rtrim(ri.[Name])) as 'data()'
			from Authorizations.[Authorization] a
			inner join Authorizations.[Role] ro on ro.Id = a.RoleId
			inner join Authorizations.[RoleRight] rr on rr.RoleId = ro.Id
			inner join Authorizations.[Right] ri on ri.Id = rr.RightId
			where a.ScopeId = sh.ParentId
			and a.PrincipalId = @principalId
			for xml path('')),' #!',','), 1, 2, '') as Names
		) Rights
	)
	select @inheritedRights = (stuff(replace((select '#!' + ltrim(rtrim(cte.ExplicitRights)) as 'data()'
	from ParentScopes_CTE cte
	for xml path('')),' #!',','), 1, 2, ''));

	return @inheritedRights;
end