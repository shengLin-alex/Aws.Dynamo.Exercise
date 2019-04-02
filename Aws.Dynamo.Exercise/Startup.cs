using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Dynamo.Exercise
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                   .AddJsonFile("appsettings.json", true, true)
                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                   .AddEnvironmentVariables();
            
            this.Configuration = builder.Build();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDefaultAWSOptions(this.Configuration.GetAWSOptions());
            
            this.SetAwsEnvVariables();

            IConfigurationSection dynamoDbConfig = this.Configuration.GetSection("DynamoDb");
            bool isRunLocalDynamoDb = dynamoDbConfig.GetValue<bool>("LocalMode");

            if (isRunLocalDynamoDb)
            {
                services.AddSingleton<IAmazonDynamoDB>(s =>
                {
                    AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig
                    {
                        ServiceURL = dynamoDbConfig.GetValue<string>("LocalServiceUrl")
                    };

                    return new AmazonDynamoDBClient(clientConfig);
                });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
        private void SetAwsEnvVariables()
        {
            AWSOptions awsOptions = this.Configuration.GetAWSOptions();
            CredentialProfileStoreChain chain = new CredentialProfileStoreChain(awsOptions.ProfilesLocation);

            if (chain.TryGetAWSCredentials(awsOptions.Profile, out AWSCredentials result))
            {
                ImmutableCredentials credentials = result.GetCredentials();
                
                Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", credentials.AccessKey);
                Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", credentials.SecretKey);
            }
            else
            {
                throw new Exception("Could not get Amazon credentials");
            }
            
            Environment.SetEnvironmentVariable("AWS_REGION", awsOptions.Region.SystemName);
        }
    }
}