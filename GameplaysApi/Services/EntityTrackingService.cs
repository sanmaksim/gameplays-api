using GameplaysApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Services
{
    public class EntityTrackingService
    {
        private readonly ApplicationDbContext _context;
        public EntityTrackingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Method to attach or update related entities
        public async Task AttachOrUpdateEntityAsync<TMain, TRelated>(
            TMain mainEntity, 
            List<TRelated>? relatedEntities,
            string uniqueIdentifierProperty,
            bool toUpdate)
            where TMain : class
            where TRelated : class
        {
            if (relatedEntities != null)
            {
                for (int i = 0; i < relatedEntities.Count; i++)
                {
                    var relatedEntity = relatedEntities[i];

                    // Get the unique identifier for the related entity (e.g., DeveloperId, PlatformId)
                    var uniqueIdProperty = relatedEntity.GetType().GetProperty(uniqueIdentifierProperty);

                    if (uniqueIdProperty == null)
                    {
                        throw new ArgumentException($"Property {uniqueIdentifierProperty} not found in type {typeof(TRelated).Name}");
                    }

                    // Get the value of the unique identifier
                    var uniqueIdValue = uniqueIdProperty.GetValue(relatedEntity);

                    // Find the related entity by its unique identifier
                    var existingEntity = await _context
                                                .Set<TRelated>()
                                                .Where(e => EF.Property<object>(e, uniqueIdentifierProperty).Equals(uniqueIdValue))
                                                .FirstOrDefaultAsync();

                    if (existingEntity != null)
                    {
                        if (toUpdate == true)
                        {
                            // Copy relevant properties from the related API entity to the related tracked entity
                            foreach (var property in _context.Entry(existingEntity).Properties)
                            {
                                if (!property.Metadata.IsPrimaryKey())
                                {
                                    property.CurrentValue = typeof(TRelated).GetProperty(property.Metadata.Name)?.GetValue(relatedEntity);
                                }
                            }
                        }
                        else
                        {
                            _context.Entry(existingEntity).State = EntityState.Unchanged;
                        }

                        // Reassign the reference in the main entity's collection
                        var relatedEntityProperty = typeof(TMain).GetProperty(typeof(TRelated).Name + "s");

                        if (relatedEntityProperty != null)
                        {
                            // Get the collection as List of related entities
                            var collection = relatedEntityProperty.GetValue(mainEntity) as List<TRelated>;

                            if (collection != null)
                            {
                                // Use the index to update the reference in the collection
                                collection[i] = existingEntity;  // Replace the existing reference with the one from the context
                            }
                        }
                    }
                    else
                    {
                        // If the entity doesn't exist, add it to the context
                        _context.Set<TRelated>().Add(relatedEntity);
                    }
                }
            }
        }
    }
}
