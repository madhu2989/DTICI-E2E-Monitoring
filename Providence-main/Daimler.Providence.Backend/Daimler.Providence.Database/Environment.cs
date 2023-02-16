using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Database
{
    [Table("Environment", Schema = "dbo")]
    [ExcludeFromCodeCoverage]
    public class Environment
    {
        public Environment()
        {
            AlertIgnores = new List<AlertIgnore>();
            IsDemo = false;
            Changelogs = new List<Changelog>();
            Checks = new List<Check>();
            Configurations = new List<Configuration>();
            StateTransitions = new List<StateTransition>();
            Deployments = new List<Deployment>();
            InternalJobs = new List<InternalJob>();
            NotificationConfigurations = new List<NotificationConfiguration>(); 
            Services = new List<Service>();
            StateIncreaseRules = new List<StateIncreaseRules>();
            StateTransitionHistories = new List<StateTransitionHistory>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ElementId { get; set; }

        public bool? IsDemo { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<AlertIgnore> AlertIgnores { get; set; }
        public virtual ICollection<Changelog> Changelogs { get; set; }
        public virtual ICollection<Check> Checks { get; set; }
        public virtual ICollection<Configuration> Configurations { get; set; }
        public virtual ICollection<Deployment> Deployments { get; set; }
        public virtual ICollection<InternalJob> InternalJobs { get; set; }
        public virtual ICollection<NotificationConfiguration> NotificationConfigurations { get; set; } 
        public virtual ICollection<Service> Services { get; set; } 
        public virtual ICollection<StateIncreaseRules> StateIncreaseRules { get; set; }
        public virtual ICollection<StateTransition> StateTransitions { get; set; } 
        public virtual ICollection<StateTransitionHistory> StateTransitionHistories { get; set; } 
    }
}
