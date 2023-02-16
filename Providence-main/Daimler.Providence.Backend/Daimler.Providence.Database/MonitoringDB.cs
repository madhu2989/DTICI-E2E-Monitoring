using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class MonitoringDB : DbContext
    {
        //public MonitoringDB(){}

        private IConfiguration _configuration;

        public MonitoringDB(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MonitoringDB(DbContextOptions<MonitoringDB> options) : base(options) { }

        #region DB Tables
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<AlertComment> AlertComments { get; set; }
        public virtual DbSet<AlertIgnore> AlertIgnores { get; set; }
        public virtual DbSet<Changelog> Changelogs { get; set; }
        public virtual DbSet<Check> Checks { get; set; }
        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentType> ComponentTypes { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<Deployment> Deployments { get; set; }
        public virtual DbSet<Environment> Environments { get; set; }
        public virtual DbSet<InternalJob> InternalJobs { get; set; }
        public virtual DbSet<MappingActionComponent> MappingActionComponents { get; set; }
        public virtual DbSet<MappingComponentTypeNotification> MappingComponentTypeNotifications { get; set; }
        public virtual DbSet<MappingStateNotification> MappingStateNotifications { get; set; }
        public virtual DbSet<NotificationConfiguration> NotificationConfigurations { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<StateIncreaseRules> StateIncreaseRules { get; set; }
        public virtual DbSet<StateTransition> StateTransitions { get; set; }
        public virtual DbSet<StateTransitionHistory> StateTransitionHistories { get; set; }
        public virtual DbSet<GetAllElementsWithEnvironmentIdReturnModel> GetAllElementsWithEnvironmentIdReturnModel { get; set; }
        public virtual DbSet<GetChangelogsCountReturnModel> GetChangelogsCountReturnModel { get; set; }
        public virtual DbSet<GetChecksToResetReturnModel> GetChecksToResetReturnModel { get; set; }
        public virtual DbSet<GetCurrentAlertIgnoresReturnModel> GetCurrentAlertIgnoresReturnModel { get; set; }
        public virtual DbSet<GetCurrentDeploymentsReturnModel> GetCurrentDeploymentsReturnModel { get; set; }
        public virtual DbSet<GetDeploymentHistoryReturnModel> GetDeploymentHistoryReturnModel { get; set; }
        public virtual DbSet<GetFutureDeploymentsReturnModel> GetFutureDeploymentsReturnModel { get; set; }
        public virtual DbSet<GetInitialStateByElementIdReturnModel> GetInitialStateByElementIdReturnModel { get; set; }
        public virtual DbSet<GetStatesReturnModel> GetStatesReturnModel { get; set; }
        public virtual DbSet<GetStateTransistionsCountReturnModel> GetStateTransistionsCountReturnModel { get; set; }
        public virtual DbSet<GetStateTransitionByIdReturnModel> GetStateTransitionByIdReturnModel { get; set; }
        public virtual DbSet<GetStateTransitionHistoryByElementIdReturnModel> GetStateTransitionHistoryByElementIdReturnModel { get; set; }
        public virtual DbSet<GetStateTransitionHistoryReturnModel> GetStateTransitionHistoryReturnModel { get; set; }

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //var configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.json", false, true)
                //.Build();

                //var connectionString = configuration.GetConnectionString("DatabaseConnectionString");
                var connectionString = _configuration.GetConnectionString("DatabaseConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Action>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.ServiceId).HasDatabaseName("nci_wi_Action_C06ABD7A01FB77E8E6AC5F034B35BEE3");
                entity.HasIndex(e => new { e.ElementId, e.EnvironmentId }).HasDatabaseName("UC_Action_ElementId_Environment").IsUnique();

                // Porperties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(250);
                entity.Property(e => e.ElementId).HasMaxLength(500);
                entity.Property(e => e.CreateDate).HasColumnType("datetime").HasDefaultValueSql("('2020-01-01')");

                // 1:n mapping
                entity.HasOne(e => e.Service)
                    .WithMany(e => e.Actions)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Action_Service");

                // M:n mapping
                entity.HasMany(e => e.Components).WithMany(e => e.Actions).UsingEntity<MappingActionComponent>(
                    e => e
                    .HasOne(e => e.Component)
                    .WithMany(e => e.MappingActionComponents)
                    .HasForeignKey(e => e.ComponentId),
                    e => e
                    .HasOne(e => e.Action)
                    .WithMany(e => e.MappingActionComponents)
                    .HasForeignKey(e => e.ActionId))
                .ToTable("Mapping_Action_Component", "dbo")
                .HasKey(e => new { e.ActionId, e.ComponentId });
            });

            modelBuilder.Entity<AlertComment>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.User).IsRequired().IsUnicode(false);
                entity.Property(e => e.Comment).IsRequired().IsUnicode(false);
                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                // 1:n mapping
                entity.HasOne(e => e.StateTransition)
                    .WithMany(e => e.AlertComment)
                    .HasForeignKey(e => e.StateTransitionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StateTransitionId");
            });

            modelBuilder.Entity<AlertIgnore>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
                entity.Property(e => e.CreationDate).HasColumnType("datetime");
                entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
                entity.Property(e => e.IgnoreCondition).IsRequired();
                entity.Property(e => e.EnvironmentSubscriptionId).HasMaxLength(500).IsRequired();

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.AlertIgnores)
                    .HasPrincipalKey(e => e.ElementId)
                    .HasForeignKey(e => e.EnvironmentSubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AlertIgnore_Environment");
            });

            modelBuilder.Entity<Changelog>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.ChangeDate).HasColumnType("datetime");
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.Changelogs)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Changelog_EnvironmentId");
            });

            modelBuilder.Entity<Check>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ElementId, e.EnvironmentId }).HasDatabaseName("UC_Check_ElementId_Env").IsUnique();

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(250);
                entity.Property(e => e.ElementId).HasMaxLength(500);

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                       .WithMany(e => e.Checks)
                       .HasForeignKey(e => e.EnvironmentId)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasConstraintName("FK_Check_Environment");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ElementId, e.EnvironmentId }).HasDatabaseName("UC_Component_ElementId_Env").IsUnique();
                entity.HasIndex(e => new { e.EnvironmentId, e.ElementId }).HasDatabaseName("nci_wi_Component_00C436ABD5B627BB32086EEA0047ED07");

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(250);
                entity.Property(e => e.ElementId).HasMaxLength(500);
                entity.Property(e => e.ComponentType).HasMaxLength(250);
                entity.Property(e => e.CreateDate).HasColumnType("datetime").HasDefaultValueSql("('2020-01-01')");

                // M:n mapping
                entity.HasMany(e => e.Actions).WithMany(e => e.Components).UsingEntity<MappingActionComponent>(
                e => e
                    .HasOne(e => e.Action)
                    .WithMany(e => e.MappingActionComponents)
                    .HasForeignKey(e => e.ActionId),
                e => e
                    .HasOne(e => e.Component)
                    .WithMany(e => e.MappingActionComponents)
                    .HasForeignKey(e => e.ComponentId))
                .ToTable("Mapping_Action_Component", "dbo")
                .HasKey(e => new { e.ComponentId, e.ActionId });
            });

            modelBuilder.Entity<ComponentType>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.Name).HasDatabaseName("UC_Name").IsUnique();

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(500);

                // M:n mapping
                entity.HasMany(e => e.NotificationConfigurations).WithMany(e => e.ComponentTypes).UsingEntity<MappingComponentTypeNotification>(
                    e => e
                    .HasOne(e => e.Notification)
                    .WithMany(e => e.MappingComponentTypeNotifications)
                    .HasForeignKey(e => e.NotificationId),
                    e => e
                    .HasOne(e => e.ComponentType)
                    .WithMany(e => e.MappingComponentTypeNotifications)
                    .HasForeignKey(e => e.ComponentTypeId))
                .ToTable("Mapping_ComponentType_Notification", "dbo")
                .HasKey(e => new { e.ComponentTypeId, e.NotificationId });
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Key).HasMaxLength(100).IsRequired();

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.Configurations)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Configuration_EnvironmentId");
            });

            modelBuilder.Entity<Deployment>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.ShortDescription).HasMaxLength(50);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.Deployments)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EnvironmentId");
            });

            modelBuilder.Entity<InternalJob>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.UserName).HasMaxLength(50);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.QueuedDate).HasColumnType("datetime");
                
                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.InternalJobs)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EnvironmentId");
            });

            modelBuilder.Entity<Environment>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.Id).HasDatabaseName("PK_Environment").IsUnique().IsClustered();
                entity.HasIndex(e => e.ElementId).HasDatabaseName("UC_Environment_ElementId").IsUnique();
                entity.HasIndex(e => e.Name).HasDatabaseName("UC_Environment_Name").IsUnique();

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(250).IsRequired();
                entity.Property(e => e.ElementId).HasMaxLength(500).IsRequired();
                entity.Property(e => e.IsDemo).HasDefaultValueSql("((0))");
                entity.Property(e => e.CreateDate).HasColumnType("datetime").HasDefaultValueSql("('2020-01-01')");
            });

            modelBuilder.Entity<NotificationConfiguration>(entity =>
            {
                // Porperties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.EmailAddresses).IsUnicode(false);

                // 1:n mapping
                entity.HasOne(e => e.Environment_Environment)
                   .WithMany(e => e.NotificationConfigurations)
                   .HasForeignKey(e => e.Environment)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_NotificationConfiguration_Environment");

                // M:n mapping
                entity.HasMany(e => e.ComponentTypes).WithMany(e => e.NotificationConfigurations).UsingEntity<MappingComponentTypeNotification>(
                   e => e
                       .HasOne(e => e.ComponentType)
                       .WithMany(e => e.MappingComponentTypeNotifications)
                       .HasForeignKey(e => e.ComponentTypeId),
                   e => e
                       .HasOne(e => e.Notification)
                       .WithMany(e => e.MappingComponentTypeNotifications)
                       .HasForeignKey(e => e.NotificationId))
                   .ToTable("Mapping_ComponentType_Notification", "dbo")
                   .HasKey(e => new { e.NotificationId, e.ComponentTypeId });

                entity.HasMany(e => e.States).WithMany(e => e.NotificationConfigurations).UsingEntity<MappingStateNotification>(
                   e => e
                       .HasOne(e => e.State)
                       .WithMany(e => e.MappingStateNotifications)
                       .HasForeignKey(e => e.StateId),
                   e => e
                       .HasOne(e => e.Notification)
                       .WithMany(e => e.MappingStateNotifications)
                       .HasForeignKey(e => e.NotificationId))
                   .ToTable("Mapping_State_Notification", "dbo")
                   .HasKey(e => new { e.NotificationId, e.StateId });
            });

            modelBuilder.Entity<Service>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ElementId, e.EnvironmentId }).HasDatabaseName("UC_Service_ElementId_EnvironmentId").IsUnique();

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(250).IsRequired();
                entity.Property(e => e.ElementId).HasMaxLength(500);
                entity.Property(e => e.CreateDate).HasColumnType("datetime").HasDefaultValueSql("('2020-01-01')");

                // 1:n mapping
                entity.HasOne(e => e.Environment)
                    .WithMany(e => e.Services)
                    .HasForeignKey(e => e.EnvironmentRef)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Service_EnvironmentId");
            });

            modelBuilder.Entity<State>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.Name).HasDatabaseName("UC_State_Name").IsUnique();

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();

                // M:n mapping
                entity.HasMany(e => e.NotificationConfigurations).WithMany(e => e.States).UsingEntity<MappingStateNotification>(
                e => e
                    .HasOne(e => e.Notification)
                    .WithMany(e => e.MappingStateNotifications)
                    .HasForeignKey(e => e.NotificationId),
                e => e
                    .HasOne(e => e.State)
                    .WithMany(e => e.MappingStateNotifications)
                    .HasForeignKey(e => e.StateId))
                .ToTable("Mapping_State_Notification", "dbo")
                .HasKey(e => new { e.StateId, e.NotificationId });
            });

            modelBuilder.Entity<StateIncreaseRules>(entity =>
            {
                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.EnvironmentSubscriptionId).HasMaxLength(500).IsRequired();
                entity.Property(e => e.CheckId).HasMaxLength(500);
                entity.Property(e => e.AlertName).HasMaxLength(500);
                entity.Property(e => e.ComponentId).HasMaxLength(500);

                // 1:n mapping
                entity.HasOne(e => e.EnvironmentSubscription)
                    .WithMany(e => e.StateIncreaseRules)
                    .HasPrincipalKey(e => e.ElementId)
                    .HasForeignKey(e => e.EnvironmentSubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StateIncreaseRule_Environment");
            });

            modelBuilder.Entity<StateTransition>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.SourceTimestamp)
                    .HasDatabaseName("IDX_SourceTimestamp");
                entity.HasIndex(e => new { e.ElementId, e.CheckId, e.AlertName })
                    .HasDatabaseName("IDX_ElementId_CheckId_Alertname");
                entity.HasIndex(e => new { e.Environment })
                    .IncludeProperties(e => new { e.SourceTimestamp, e.CheckId, e.ElementId, e.AlertName })
                    .HasDatabaseName("IDX_StateTransition_042F846098DC4328AD8C334A2BB3E6431");
                entity.HasIndex(e => new { e.Guid })
                    .IncludeProperties(e => new { e.AlertName, e.CheckId, e.ComponentType, e.Customfield1, e.Customfield2, e.Customfield3, e.Customfield4, e.Customfield5, e.Description, e.ElementId, e.Environment, e.GeneratedTimestamp, e.ProgressState, e.SourceTimestamp, e.State, e.TriggeredByAlertName, e.TriggeredByCheckId, e.TriggeredByElementId })
                    .HasDatabaseName("nci_wi_StateTransition_85BA8B1A62E2B22C0F5E162114C6DE83");

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.SourceTimestamp).HasColumnType("datetime");
                entity.Property(e => e.GeneratedTimestamp).HasColumnType("datetime");
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Customfield1).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Customfield2).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Customfield3).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Customfield4).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Customfield5).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CheckId).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.Guid).HasMaxLength(500);
                entity.Property(e => e.ElementId).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.AlertName).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.TriggeredByCheckId).HasMaxLength(500);
                entity.Property(e => e.TriggeredByElementId).HasMaxLength(500);
                entity.Property(e => e.TriggeredByAlertName).HasMaxLength(500);
                entity.Property(e => e.ProgressState).HasDefaultValueSql("((0))");

                // 1:n mapping
                entity.HasOne(e => e.ComponentType_ComponentType)
                    .WithMany(e => e.StateTransitions)
                    .HasForeignKey(e => e.ComponentType)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__StateTran__Compo__24BD5A91");

                entity.HasOne(e => e.Environment_Environment)
                    .WithMany(e => e.StateTransitions)
                    .HasForeignKey(e => e.Environment)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__StateTran__Envir__23C93658");

                entity.HasOne(e => e.State_State)
                    .WithMany(e => e.StateTransitions)
                    .HasForeignKey(e => e.State)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__StateTran__State__22D5121F");
            });

            modelBuilder.Entity<StateTransitionHistory>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ElementId, e.EnvironmentId })
                        .IncludeProperties(e => new { e.ComponentType, e.EndDate, e.StartDate, e.State })
                        .HasDatabaseName("nci_wi_StateTransitionHistory_28059F6C2029E9D031919FBE15775ED4");
                entity.HasIndex(e => new { e.EnvironmentId, e.ComponentType, e.EndDate, e.StartDate, e.State })
                        .IncludeProperties(e => e.ElementId)
                        .HasDatabaseName("nci_wi_StateTransitionHistory_6EB18D29F379FB68637F9FA78B4071E1");

                // Properties
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.ElementId).HasMaxLength(500).IsRequired();
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");

                // 1:n mapping
                entity.HasOne(e => e.ComponentType_ComponentType)
                    .WithMany(e => e.StateTransitionHistories)
                    .HasForeignKey(e => e.ComponentType)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StateTransitionHistory_ComponentType");

                entity.HasOne(e => e.Environment_Environment)
                    .WithMany(e => e.StateTransitionHistories)
                    .HasForeignKey(e => e.EnvironmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StateTransitionHistory_EnvironmentId");

                entity.HasOne(e => e.State_State)
                    .WithMany(e => e.StateTransitionHistories)
                    .HasForeignKey(e => e.State)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StateTransitionHistory_State");
            });

            // Functions / Stored Procedures
            modelBuilder.Entity<GetAllElementsWithEnvironmentIdReturnModel>().HasNoKey();
            modelBuilder.Entity<GetChangelogsCountReturnModel>().HasNoKey();
            modelBuilder.Entity<GetChecksToResetReturnModel>().HasNoKey();
            modelBuilder.Entity<GetCurrentAlertIgnoresReturnModel>().HasNoKey();
            modelBuilder.Entity<GetCurrentDeploymentsReturnModel>().HasNoKey();
            modelBuilder.Entity<GetDeploymentHistoryReturnModel>().HasNoKey();
            modelBuilder.Entity<GetFutureDeploymentsReturnModel>().HasNoKey();
            modelBuilder.Entity<GetInitialStateByElementIdReturnModel>().HasNoKey();
            modelBuilder.Entity<GetStatesReturnModel>().HasNoKey();
            modelBuilder.Entity<GetStateTransistionsCountReturnModel>().HasNoKey();
            modelBuilder.Entity<GetStateTransitionByIdReturnModel>().HasNoKey();
            modelBuilder.Entity<GetStateTransitionHistoryByElementIdReturnModel>().HasNoKey();
            modelBuilder.Entity<GetStateTransitionHistoryReturnModel>().HasNoKey();

            OnModelCreatingPartial(modelBuilder);
        }

        #region Functions / Stored Procedures

        public async Task<List<GetAllElementsWithEnvironmentIdReturnModel>> GetAllElementsWithEnvironmentId(int? environmentId, CancellationToken token)
        {
            var queryResult = new List<GetAllElementsWithEnvironmentIdReturnModel>();
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetAllElementsWithEnvironmentId](@envId)";
            queryResult = await this.GetAllElementsWithEnvironmentIdReturnModel
                .FromSqlRaw(sqlQuery, envParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetChangelogsCountReturnModel>> GetChangelogsCount()
        {
            var queryResult = new List<GetChangelogsCountReturnModel>();
            var sqlQuery = "SELECT * FROM [dbo].[GetChangelogsCount]()";
            queryResult = await this.GetChangelogsCountReturnModel
                .FromSqlRaw(sqlQuery)
                .ToListAsync()
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetChecksToResetReturnModel>> GetChecksToReset(int? environmentId)
        {
            var queryResult = new List<GetChecksToResetReturnModel>();
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetChecksToReset](@envId)";
            queryResult = await this.GetChecksToResetReturnModel
                .FromSqlRaw(sqlQuery, envParam)
                .ToListAsync()
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetCurrentAlertIgnoresReturnModel>> GetCurrentAlertIgnores(string environmentSubscriptionId, CancellationToken token)
        {
            var queryResult = new List<GetCurrentAlertIgnoresReturnModel>();
            var envParam = new SqlParameter("@EnvironmentSubscriptionId", environmentSubscriptionId ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetCurrentAlertIgnores](@EnvironmentSubscriptionId)";
            queryResult = await this.GetCurrentAlertIgnoresReturnModel
            .FromSqlRaw(sqlQuery, envParam)
            .ToListAsync()
            .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetCurrentDeploymentsReturnModel>> GetCurrentDeployments(int? environmentId, CancellationToken token)
        {
            var queryResult = new List<GetCurrentDeploymentsReturnModel>();
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetCurrentDeployments](@envId)";
            queryResult = await this.GetCurrentDeploymentsReturnModel
                .FromSqlRaw(sqlQuery, envParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetDeploymentHistoryReturnModel>> GetDeploymentHistory(int? environmentId, DateTime? startDate, DateTime? endDate, CancellationToken token)
        {
            var queryResult = new List<GetDeploymentHistoryReturnModel>();
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var startDateParam = new SqlParameter("@startDateParam", startDate ?? (object)DBNull.Value);
            var endDateParam = new SqlParameter("@endDateParam", endDate ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetDeploymentHistory](@envId,@startDateParam,@endDateParam)";
            queryResult = await this.GetDeploymentHistoryReturnModel
                .FromSqlRaw(sqlQuery, envParam, startDateParam, endDateParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetFutureDeploymentsReturnModel>> GetFutureDeployments(int? environmentId, CancellationToken token)
        {
            var queryResult = new List<GetFutureDeploymentsReturnModel>();
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetFutureDeployments](@envId)";
            queryResult = await this.GetFutureDeploymentsReturnModel
                .FromSqlRaw(sqlQuery, envParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetInitialStateByElementIdReturnModel>> GetInitialStateByElementId(string elementId, int? environment, System.DateTime? referenceDate, CancellationToken token)
        {
            List<GetInitialStateByElementIdReturnModel> queryResult = new List<GetInitialStateByElementIdReturnModel>();
            var elementIdParam = new SqlParameter("@elementId", elementId ?? (object)DBNull.Value);
            var envParam = new SqlParameter("@envId", environment.ToString() ?? (object)DBNull.Value);
            var referenceDateParam = new SqlParameter("@referenceDateParam", referenceDate ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetInitialStateByElementId](@elementId, @envId, @referenceDateParam)";
            queryResult = await this.GetInitialStateByElementIdReturnModel
                .FromSqlRaw(sqlQuery, elementIdParam, envParam, referenceDateParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetStatesReturnModel>> GetStates(int? environment, DateTime? referenceDate, CancellationToken token)
        {
            var queryResult = new List<GetStatesReturnModel>();
            var envParam = new SqlParameter("@envId", environment.ToString() ?? (object)DBNull.Value);
            var referenceDateParam = new SqlParameter("@referenceDateParam", referenceDate ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetStates](@envId, @referenceDateParam)";
            queryResult = await this.GetStatesReturnModel
                .FromSqlRaw(sqlQuery, envParam, referenceDateParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetStateTransistionsCountReturnModel>> GetStateTransistionsCount()
        {
            var queryResult = new List<GetStateTransistionsCountReturnModel>();
            var sqlQuery = "SELECT * FROM [dbo].[GetStateTransistionsCount]()";
            queryResult = await this.GetStateTransistionsCountReturnModel
                .FromSqlRaw(sqlQuery)
                .ToListAsync()
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetStateTransitionByIdReturnModel>> GetStateTransitionById(int? id, CancellationToken token)
        {
            var queryResult = new List<GetStateTransitionByIdReturnModel>();
            var idParam = new SqlParameter("@envId", id.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetStateTransitionById](@envId)";
            queryResult = await this.GetStateTransitionByIdReturnModel
                .FromSqlRaw(sqlQuery, idParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetStateTransitionHistoryByElementIdReturnModel>> GetStateTransitionHistoryByElementId(string elementId, int? environment, DateTime? startDate, System.DateTime? endDate, CancellationToken token)
        {
            var queryResult = new List<GetStateTransitionHistoryByElementIdReturnModel>();
            var elementIdParam = new SqlParameter("@elementId", elementId ?? (object)DBNull.Value);
            var envParam = new SqlParameter("@envId", environment.ToString() ?? (object)DBNull.Value);
            var startDateParam = new SqlParameter("@startDateParam", startDate ?? (object)DBNull.Value);
            var endDateParam = new SqlParameter("@endDateParam", endDate ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetStateTransitionHistoryByElementId](@elementId, @envId, @startDateParam, @endDateParam)";
            queryResult = await this.GetStateTransitionHistoryByElementIdReturnModel
                .FromSqlRaw(sqlQuery, elementIdParam, envParam, startDateParam, endDateParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        public async Task<List<GetStateTransitionHistoryReturnModel>> GetStateTransitionHistory(int? environment, DateTime? startDate, DateTime? endDate, CancellationToken token)
        {
            var queryResult = new List<GetStateTransitionHistoryReturnModel>();
            var envParam = new SqlParameter("@envId", environment.ToString() ?? (object)DBNull.Value);
            var startDateParam = new SqlParameter("@startDateParam", startDate ?? (object)DBNull.Value);
            var endDateParam = new SqlParameter("@endDateParam", endDate ?? (object)DBNull.Value);
            var sqlQuery = "SELECT * FROM [dbo].[GetStateTransitionHistory](@envId, @startDateParam, @endDateParam)";
            queryResult = await this.GetStateTransitionHistoryReturnModel
                .FromSqlRaw(sqlQuery, envParam, startDateParam, endDateParam)
                .ToListAsync(token)
                .ConfigureAwait(false);
            return queryResult;
        }

        // Stored Procedures
        public async Task<int> CleanEnvironment(int? environmentId)
        {
            int result;
            var envParam = new SqlParameter("@envId", environmentId.ToString() ?? (object)DBNull.Value);
            var sqlQuery = "EXECUTE [dbo].[CleanEnvironment] @envId";
            result = await this.Database.ExecuteSqlRawAsync(sqlQuery, envParam).ConfigureAwait(false);
            return result;
        }

        public async Task<int> DeleteExpiredChangelogs(DateTime? cutOffDate)
        {
            int result;
            var envParam = new SqlParameter("@cutOffDate", cutOffDate ?? (object)DBNull.Value);
            var sqlQuery = "EXECUTE [dbo].[DeleteExpiredChangelogs] @cutOffDate";
            result = await this.Database.ExecuteSqlRawAsync(sqlQuery, envParam).ConfigureAwait(false);
            return result;
        }

        public async Task<int> DeleteExpiredStatetransitions(DateTime? cutOffDate)
        {
            int result;
            var envParam = new SqlParameter("@cutOffDate", cutOffDate ?? (object)DBNull.Value);
            var sqlQuery = "EXECUTE [dbo].[DeleteExpiredStatetransitions] @cutOffDate";
            result = await this.Database.ExecuteSqlRawAsync(sqlQuery, envParam).ConfigureAwait(false);
            return result;
        }

        public async Task<int> DeleteUnusedComponents()
        {
            int result;
            var sqlQuery = "EXEC [dbo].[DeleteUnusedComponents]";
            result = await this.Database.ExecuteSqlRawAsync(sqlQuery).ConfigureAwait(false);
            return result;
        }

        #endregion

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
