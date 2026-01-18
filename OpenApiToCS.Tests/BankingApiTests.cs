using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;

namespace OpenApiToCS.Tests;

public class BankingApiTests
{
    private readonly OpenApiDocument _document;

    public BankingApiTests()
    {
        string json = File.ReadAllText("TestData/banking.json");
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
        };
        _document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
    }

    [Fact]
    public void Should_Deserialize_Banking_OpenApi_Document()
    {
        _document.ShouldNotBeNull();
        _document.OpenApiVersion.ShouldBe("3.0.0");
        _document.Info.Title.ShouldBe("Banking System API");
        _document.Info.Version.ShouldBe("3.2.1");
    }

    [Fact]
    public void Should_Generate_Complex_CustomerDetail_Model()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("CustomerDetail");
        var customerClass = result.Classes["CustomerDetail"];
        
        // Verify complex model structure
        customerClass.Source.ShouldContain("public record CustomerDetail");
        customerClass.Source.ShouldContain("public Guid Id");
        customerClass.Source.ShouldContain("public string Email");
        customerClass.Source.ShouldContain("public string? MiddleName");
        customerClass.Source.ShouldContain("public DateOnly DateOfBirth");
        customerClass.Source.ShouldContain("public Address Address");
        customerClass.Source.ShouldContain("public CustomerStatus Status");
        customerClass.Source.ShouldContain("public CustomerTier Tier");
        customerClass.Source.ShouldContain("public Account[] Accounts");
        customerClass.Source.ShouldContain("public RiskProfile RiskProfile");
        customerClass.Source.ShouldContain("public CustomerPreferences Preferences");
        customerClass.Source.ShouldContain("public DateTimeOffset CreatedAt");
        customerClass.Source.ShouldContain("public DateTimeOffset? LastLoginAt");
        
        // Verify deprecated property
        customerClass.Source.ShouldContain("[Obsolete(\"This property is deprecated.\")]");
        customerClass.Source.ShouldContain("public string TaxId");
    }

    [Fact]
    public void Should_Generate_Nested_Address_With_GeoCoordinates()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Address");
        result.Classes.Keys.ShouldContain("GeoCoordinates");
        
        var addressClass = result.Classes["Address"];
        addressClass.Source.ShouldContain("public GeoCoordinates Coordinates");
        addressClass.Source.ShouldContain("public string? Street2");
        
        var coordClass = result.Classes["GeoCoordinates"];
        coordClass.Source.ShouldContain("public double Latitude");
        coordClass.Source.ShouldContain("public double Longitude");
    }

    [Fact]
    public void Should_Generate_All_Customer_Related_Enums()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        // Customer enums
        result.Classes.Keys.ShouldContain("CustomerStatus");
        result.Classes["CustomerStatus"].Source.ShouldContain("pending_verification,");
        
        result.Classes.Keys.ShouldContain("CustomerTier");
        result.Classes["CustomerTier"].Source.ShouldContain("private_banking,");
        
        result.Classes.Keys.ShouldContain("RiskLevel");
        result.Classes["RiskLevel"].Source.ShouldContain("very_high,");
    }

    [Fact]
    public void Should_Generate_Deeply_Nested_Preferences()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("CustomerPreferences");
        result.Classes.Keys.ShouldContain("NotificationPreferences");
        
        var prefsClass = result.Classes["CustomerPreferences"];
        prefsClass.Source.ShouldContain("public NotificationPreferences Notifications");
        
        var notifClass = result.Classes["NotificationPreferences"];
        notifClass.Source.ShouldContain("public bool Email");
        notifClass.Source.ShouldContain("public bool TransactionAlerts");
        notifClass.Source.ShouldContain("public bool SecurityAlerts");
    }

    [Fact]
    public void Should_Generate_Account_Model_With_Money_Type()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Account");
        result.Classes.Keys.ShouldContain("Money");
        result.Classes.Keys.ShouldContain("AccountType");
        result.Classes.Keys.ShouldContain("AccountStatus");
        
        var accountClass = result.Classes["Account"];
        accountClass.Source.ShouldContain("public Money Balance");
        accountClass.Source.ShouldContain("public Money AvailableBalance");
        accountClass.Source.ShouldContain("public double? InterestRate");
        accountClass.Source.ShouldContain("public DateTimeOffset? ClosedAt");
        
        var moneyClass = result.Classes["Money"];
        moneyClass.Source.ShouldContain("public double Amount");
        moneyClass.Source.ShouldContain("public string Currency");
    }

    [Fact]
    public void Should_Generate_AccountListResponse_With_Pagination()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("AccountListResponse");
        result.Classes.Keys.ShouldContain("PaginationInfo");
        result.Classes.Keys.ShouldContain("AccountSummary");
        
        var responseClass = result.Classes["AccountListResponse"];
        responseClass.Source.ShouldContain("public Account[] Accounts");
        responseClass.Source.ShouldContain("public PaginationInfo Pagination");
        responseClass.Source.ShouldContain("public AccountSummary Summary");
        
        var paginationClass = result.Classes["PaginationInfo"];
        paginationClass.Source.ShouldContain("public int Page");
        paginationClass.Source.ShouldContain("public int PageSize");
        paginationClass.Source.ShouldContain("public bool HasNextPage");
    }

    [Fact]
    public void Should_Generate_Transaction_With_Merchant()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Transaction");
        result.Classes.Keys.ShouldContain("Merchant");
        result.Classes.Keys.ShouldContain("TransactionType");
        result.Classes.Keys.ShouldContain("TransactionStatus");
        
        var transactionClass = result.Classes["Transaction"];
        transactionClass.Source.ShouldContain("public Merchant Merchant");
        transactionClass.Source.ShouldContain("public Guid? RelatedTransactionId");
        transactionClass.Source.ShouldContain("public DateTimeOffset? PostedAt");
        
        var merchantClass = result.Classes["Merchant"];
        merchantClass.Source.ShouldContain("public Address Location");
    }

    [Fact]
    public void Should_Generate_Investment_Portfolio_And_Holdings()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("Portfolio");
        result.Classes.Keys.ShouldContain("Holding");
        result.Classes.Keys.ShouldContain("InvestmentStrategy");
        result.Classes.Keys.ShouldContain("AssetType");
        
        var portfolioClass = result.Classes["Portfolio"];
        portfolioClass.Source.ShouldContain("public Holding[] Holdings");
        portfolioClass.Source.ShouldContain("public double TotalReturnPercentage");
        
        var holdingClass = result.Classes["Holding"];
        holdingClass.Source.ShouldContain("public AssetType AssetType");
        holdingClass.Source.ShouldContain("public double UnrealizedGainLossPercentage");
        
        var assetTypeEnum = result.Classes["AssetType"];
        assetTypeEnum.Source.ShouldContain("cryptocurrency,");
        assetTypeEnum.Source.ShouldContain("real_estate,");
    }

    [Fact]
    public void Should_Generate_KYC_Compliance_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("KycStatus");
        result.Classes.Keys.ShouldContain("KycDocument");
        result.Classes.Keys.ShouldContain("KycVerificationStatus");
        result.Classes.Keys.ShouldContain("KycLevel");
        result.Classes.Keys.ShouldContain("DocumentType");
        result.Classes.Keys.ShouldContain("DocumentStatus");
        
        var kycClass = result.Classes["KycStatus"];
        kycClass.Source.ShouldContain("public KycDocument[] Documents");
        kycClass.Source.ShouldContain("public DateTimeOffset? VerifiedAt");
        kycClass.Source.ShouldContain("public string? Notes");
        
        var docTypeEnum = result.Classes["DocumentType"];
        docTypeEnum.Source.ShouldContain("passport,");
        docTypeEnum.Source.ShouldContain("drivers_license,");
        docTypeEnum.Source.ShouldContain("tax_return,");
    }

    [Fact]
    public void Should_Generate_Loan_Application_With_Employment_Info()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        result.Classes.Keys.ShouldContain("LoanApplication");
        result.Classes.Keys.ShouldContain("LoanType");
        result.Classes.Keys.ShouldContain("Collateral");
        result.Classes.Keys.ShouldContain("EmploymentInfo");
        result.Classes.Keys.ShouldContain("EmploymentType");
        
        var loanClass = result.Classes["LoanApplication"];
        loanClass.Source.ShouldContain("public EmploymentInfo EmploymentInfo");
        loanClass.Source.ShouldContain("public Money AnnualIncome");
        loanClass.Source.ShouldContain("public int Term");
        
        var empTypeEnum = result.Classes["EmploymentType"];
        empTypeEnum.Source.ShouldContain("self_employed,");
        empTypeEnum.Source.ShouldContain("retired,");
    }

    [Fact]
    public void Should_Generate_CustomersClient_With_Header_Parameter()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("CustomersClientV3");
        var clientSource = result["CustomersClientV3"];
        
        // Should have api-version header handled
        clientSource.ShouldContain("httpRequest.Headers.Add(\"api-version\"");
        clientSource.ShouldContain("public async Task<CustomerDetail> GetCustomerId(");
        clientSource.ShouldContain("Guid customerId");
        clientSource.ShouldContain("bool? includeAccounts = null");
    }

    [Fact]
    public void Should_Generate_AccountsClient_With_Complex_Query_Parameters()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("AccountsClientV3");
        var clientSource = result["AccountsClientV3"];
        
        clientSource.ShouldContain("public async Task<AccountListResponse> GetAccounts(");
        clientSource.ShouldContain("Guid? customerId = null");
        clientSource.ShouldContain("AccountType? accountType = null");
        clientSource.ShouldContain("AccountStatus? status = null");
        clientSource.ShouldContain("double? minBalance = null");
        clientSource.ShouldContain("int? page = null");
        clientSource.ShouldContain("int? pageSize = null");
    }

    [Fact]
    public void Should_Generate_AccountsClient_With_Post_For_201_Response()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        var clientSource = result["AccountsClientV3"];
        
        // POST with 201 response
        clientSource.ShouldContain("public async Task PostAccounts(");
        clientSource.ShouldContain("AccountCreationRequest accountCreationRequest");
    }

    [Fact]
    public void Should_Generate_TransactionsClient_With_Date_Parameters()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("AccountsClientV3");
        var clientSource = result["AccountsClientV3"];
        
        clientSource.ShouldContain("public async Task<Transaction[]> GetTransactions(");
        clientSource.ShouldContain("DateOnly? startDate = null");
        clientSource.ShouldContain("DateOnly? endDate = null");
        clientSource.ShouldContain("TransactionType? transactionType = null");
    }

    [Fact]
    public void Should_Generate_InvestmentsClient_For_Nested_Routes()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("InvestmentsClientV3");
        var clientSource = result["InvestmentsClientV3"];
        
        clientSource.ShouldContain("public async Task<Portfolio[]> GetPortfolios(");
        clientSource.ShouldContain("public async Task<Holding[]> GetHoldings(");
        clientSource.ShouldContain("Guid portfolioId");
    }

    [Fact]
    public void Should_Generate_ComplianceClient_For_KYC()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("ComplianceClientV3");
        var clientSource = result["ComplianceClientV3"];
        
        clientSource.ShouldContain("public async Task<KycStatus> GetKycCustomerId(");
        clientSource.ShouldContain("public async Task<KycStatus> PostKycCustomerId(");
        clientSource.ShouldContain("KycUpdate kycUpdate");
    }

    [Fact]
    public void Should_Generate_LoansClient_With_Post_Only()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("LoansClientV3");
        var clientSource = result["LoansClientV3"];
        
        clientSource.ShouldContain("public async Task<LoanApplicationResponse> PostLoans(");
        clientSource.ShouldContain("LoanApplication loanApplication");
    }

    [Fact]
    public void Should_Generate_CardsClient_With_204_No_Content()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        result.Keys.ShouldContain("CardsClientV3");
        var clientSource = result["CardsClientV3"];
        
        // 204 response should return Task (not Task<T>)
        clientSource.ShouldContain("public async Task PostActivate(");
        clientSource.ShouldContain("CardActivationRequest cardActivationRequest");
    }

    [Fact]
    public void Should_Use_Version_3_In_All_Client_Names()
    {
        var dataClasses = new DataClassGenerator(_document).GenerateDataClasses();
        var generator = new OperationGenerator(_document, dataClasses, false);
        var result = generator.GenerateApiClasses();

        // Version is 3.2.1, should use first character (3)
        foreach (var clientName in result.Keys)
        {
            clientName.ShouldEndWith("ClientV3");
        }
    }

    [Fact]
    public void Should_Generate_Expected_Number_Of_Models()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        // This is a complex API with many models
        result.ClassCount.ShouldBeGreaterThan(30);
        
        // Verify key models exist
        var expectedModels = new[]
        {
            "CustomerDetail", "Address", "Account", "Transaction", 
            "Portfolio", "Holding", "KycStatus", "LoanApplication",
            "Money", "PaginationInfo", "AccountListResponse"
        };
        
        foreach (var model in expectedModels)
        {
            result.Classes.Keys.ShouldContain(model, $"Missing expected model: {model}");
        }
    }

    [Fact]
    public void Should_Generate_All_Expected_Enum_Types()
    {
        var generator = new DataClassGenerator(_document);
        var result = generator.GenerateDataClasses();

        var expectedEnums = new[]
        {
            "CustomerStatus", "CustomerTier", "RiskLevel", 
            "AccountType", "AccountStatus", "TransactionType", "TransactionStatus",
            "InvestmentStrategy", "AssetType",
            "KycVerificationStatus", "KycLevel", "DocumentType", "DocumentStatus",
            "LoanType", "EmploymentType", "LoanApplicationStatus"
        };
        
        foreach (var enumName in expectedEnums)
        {
            result.Classes.Keys.ShouldContain(enumName, $"Missing expected enum: {enumName}");
            result.Classes[enumName].Source.ShouldContain($"public enum {enumName}");
        }
    }
}
