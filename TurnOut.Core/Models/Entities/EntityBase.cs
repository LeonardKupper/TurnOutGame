using System;
using System.Collections.Generic;
using System.Linq;
using TurnOut.Core.Models.EntityExtensions;

namespace TurnOut.Core.Models.Entities
{

    public abstract class EntityBase
    {
        public EntityBase()
        {
            SetupBehavior();
        }

        #region Entity Extension System
        protected virtual void SetupBehavior()
        {

        }

        public List<IEntityExtension> Extensions { get; } = new List<IEntityExtension>();

        protected void AddExtension<TExtension>() where TExtension : IEntityExtension, new()
        {
            RemoveExtension<TExtension>();
            Extensions.Add(new TExtension());
        }
        protected void RemoveExtension<TExtension>() where TExtension : IEntityExtension, new()
        {
            Extensions.RemoveAll(e => e.GetType() == typeof(TExtension));
        }

        public bool Has<TExtension>() where TExtension : IEntityExtension
        {
            return Has<TExtension>(out _);
        }

        public bool Has<TExtension>(out TExtension extension) where TExtension : IEntityExtension
        {
            extension = (TExtension)Extensions.Where(t => t.GetType() == typeof(TExtension)).FirstOrDefault();
            return (extension != null);
        }
        #endregion


        public bool IsDestroyed { get; set; }
    }
}