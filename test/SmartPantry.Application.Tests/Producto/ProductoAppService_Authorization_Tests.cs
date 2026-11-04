using System;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using Volo.Abp.Identity;
using Xunit;

namespace SmartPantry.Productos;

public class ProductoAppService_Authorization_Tests
    : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
{
    [Fact]
    public void Product_app_service_requires_authentication_for_crud_operations()
    {
        var authorizeAttribute = typeof(ProductoAppService)
            .GetCustomAttribute<AuthorizeAttribute>(inherit: true);

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute!.Roles.ShouldBeNull();
        authorizeAttribute.Policy.ShouldBeNull();
    }

    [Fact]
    public void External_product_lookup_requires_authentication_without_admin_role()
    {
        var method = typeof(ProductoAppService).GetMethod(nameof(ProductoAppService.GetExternalAsync));
        method.ShouldNotBeNull();

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>(inherit: true);

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute!.Roles.ShouldBeNull();
        authorizeAttribute.Policy.ShouldBeNull();
    }

    [Fact]
    public void Identity_user_list_requires_the_identity_users_permission()
    {
        // The application service lives in the Identity application assembly; the input
        // DTO lives in the contracts assembly, so resolve the service from its own module.
        var identityApplicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .SingleOrDefault(assembly => assembly.GetName().Name == "Volo.Abp.Identity.Application");
        identityApplicationAssembly.ShouldNotBeNull();

        var identityServiceType = identityApplicationAssembly!.GetTypes()
            .SingleOrDefault(type => type.Name == "IdentityUserAppService");
        identityServiceType.ShouldNotBeNull();

        var method = identityServiceType!.GetMethods()
            .SingleOrDefault(candidate =>
                candidate.Name == "GetListAsync" &&
                candidate.GetParameters().Any(parameter =>
                    parameter.ParameterType.Name == nameof(GetIdentityUsersInput)));
        method.ShouldNotBeNull();

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>(inherit: true);

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute!.Policy.ShouldBe("AbpIdentity.Users");
    }
}
