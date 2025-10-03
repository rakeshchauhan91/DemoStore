namespace CatalogService.Api.Models.Events
{
    public class ProductPriceChanged : IntegrationEvent
    {
        public Guid ProductId { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }

        public ProductPriceChanged(Guid productId, decimal oldPrice, decimal newPrice)
        {
            ProductId = productId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
        }
    }
    public class ProductDeletedEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public string Name { get; }
        public ProductDeletedEvent(Guid productiId)
        {
            ProductId = productiId;
        }
    }
    public class ProductCreatedEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public string Name { get; }
        public string SKU { get; }

        public ProductCreatedEvent(Guid productId, string sku, string namwe)
        {
            ProductId = productId;
            Name = namwe;
            SKU = sku;
        }
    }



    public class ProductUpdated  : IntegrationEvent
    {
        public Guid Id { get; }
        public String Name { get; }


        public ProductUpdated(Guid productId, string Name)
        {
            Id = productId;
            Name = Name;
        }
    }



}
