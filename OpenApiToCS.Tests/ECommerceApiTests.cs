using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class ECommerceApiTests
{
    private readonly OpenApiDocument _document;

    public ECommerceApiTests()
    {
        string json = File.ReadAllText("TestData/ecommerce.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_ECommerce_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.0");
        _document.Info.Title.ShouldBe("E-Commerce API");
        _document.Info.Version.ShouldBe("2.0.0");
    }

    [Fact]
    public void Should_Generate_Product_Model_With_Various_Types()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Product");
        var productClass = result.Classes["Product"];
        
        productClass.Source.ShouldContain("public record Product");
        productClass.Source.ShouldContain("public long Id");
        productClass.Source.ShouldContain("public string Name");
        productClass.Source.ShouldContain("public string? Description");
        productClass.Source.ShouldContain("public double Price");
        productClass.Source.ShouldContain("public bool InStock");
        productClass.Source.ShouldContain("public string[] Tags");
        productClass.Source.ShouldContain("public DateTimeOffset CreatedAt");
    }

    [Fact]
    public void Should_Generate_Order_Model_With_Reference_Properties()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Order");
        var orderClass = result.Classes["Order"];
        
        orderClass.Source.ShouldContain("public record Order");
        orderClass.Source.ShouldContain("public Guid Id");
        orderClass.Source.ShouldContain("public OrderItem[] Items");
        orderClass.Source.ShouldContain("public double Total");
        orderClass.Source.ShouldContain("public OrderStatus Status");
    }

    [Fact]
    public void Should_Generate_OrderItem_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("OrderItem");
        var orderItemClass = result.Classes["OrderItem"];
        
        orderItemClass.Source.ShouldContain("public record OrderItem");
        orderItemClass.Source.ShouldContain("public long ProductId");
        orderItemClass.Source.ShouldContain("public int Quantity");
        orderItemClass.Source.ShouldContain("public double Price");
    }

    [Fact]
    public void Should_Generate_Address_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Address");
        var addressClass = result.Classes["Address"];
        
        addressClass.Source.ShouldContain("public record Address");
        addressClass.Source.ShouldContain("public string Street");
        addressClass.Source.ShouldContain("public string City");
        addressClass.Source.ShouldContain("public string State");
        addressClass.Source.ShouldContain("public string Country");
        addressClass.Source.ShouldContain("public string PostalCode");
        
        // Required properties should have [Required] attribute
        addressClass.Properties.First(p => p.Name == "Street").IsRequired.ShouldBeTrue();
        addressClass.Properties.First(p => p.Name == "City").IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Should_Generate_OrderRequest_Model_With_Nested_References()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("OrderRequest");
        var orderRequestClass = result.Classes["OrderRequest"];
        
        orderRequestClass.Source.ShouldContain("public record OrderRequest");
        orderRequestClass.Source.ShouldContain("public OrderItem[] Items");
        orderRequestClass.Source.ShouldContain("public Address ShippingAddress");
        orderRequestClass.Source.ShouldContain("public Address BillingAddress");
    }

    [Fact]
    public void Should_Generate_OrderStatus_Enum()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("OrderStatus");
        var enumClass = result.Classes["OrderStatus"];
        
        enumClass.Source.ShouldContain("public enum OrderStatus");
        enumClass.Source.ShouldContain("pending,");
        enumClass.Source.ShouldContain("processing,");
        enumClass.Source.ShouldContain("shipped,");
        enumClass.Source.ShouldContain("delivered,");
        enumClass.Source.ShouldContain("cancelled,");
    }

    [Fact]
    public void Should_Generate_ProductsClient_With_Optional_Query_Parameters()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("ProductsClientV2");
        var clientSource = result["ProductsClientV2"];
        
        clientSource.ShouldContain("public async Task<Product[]> GetProducts(");
        clientSource.ShouldContain("string? category = null");
        clientSource.ShouldContain("double? minPrice = null");
        clientSource.ShouldContain("double? maxPrice = null");
        clientSource.ShouldContain("queryBuilder.Add(\"category\", category.ToString());");
        clientSource.ShouldContain("queryBuilder.Add(\"minPrice\", minPrice.Value.ToString());");
    }

    [Fact]
    public void Should_Generate_OrdersClient_With_Post_Method()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("OrdersClientV2");
        var clientSource = result["OrdersClientV2"];
        
        clientSource.ShouldContain("public async Task<Order> PostOrders(");
        clientSource.ShouldContain("OrderRequest orderRequest");
        clientSource.ShouldContain("httpRequest.Content = JsonContent.Create(orderRequest");
    }

    [Fact]
    public void Should_Use_Version_2_In_Namespace()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["ProductsClientV2"];
        
        clientSource.ShouldContain("namespace ECommerceApiApiClientV2;");
        clientSource.ShouldContain("using ECommerceApiApiClientV2.Models;");
    }

    [Fact]
    public void Should_Generate_All_Expected_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Product");
        result.Classes.Keys.ShouldContain("OrderRequest");
        result.Classes.Keys.ShouldContain("Order");
        result.Classes.Keys.ShouldContain("OrderItem");
        result.Classes.Keys.ShouldContain("Address");
        result.Classes.Keys.ShouldContain("OrderStatus");
        
        result.ClassCount.ShouldBe(6);
    }
}
