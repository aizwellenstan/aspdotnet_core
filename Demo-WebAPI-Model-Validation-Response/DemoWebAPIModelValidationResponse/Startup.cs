using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoWebAPIModelValidationResponse
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddMvcOptions(options =>
                {
                    ooptions.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => L["The value '{0}' is invalid.", x]);
                    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(x => L["The value '{0}' is invalid.", x]);
                    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x => L["The field {0} must be a number.", x]);
                    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(x => L["A value for the '{0}' property was not provided.", x]);
                    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => L["The value '{0}' is not valid for {1}.", x, y]);
                    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => L["A value is required."]);
                    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(x => L["The supplied value is invalid for {0}.", x]);
                    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => L["A non-empty request body is required."]);
                    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(x => L["The value '{0}' is not valid.", x]);
                    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => L["The supplied value is invalid."]);
                    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => L["NonPropertyValueMustBeNumber"]);
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext => new BadRequestObjectResult(new { Message = "Model binding occurs problem." });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
