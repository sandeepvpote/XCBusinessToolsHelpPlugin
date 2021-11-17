/******************************************************************************
* This script should run against 'SitecoreCommerce_SharedEnvironments' and
* 'SitecoreCommerce_Global' to upgrade from Sitecore XC 9.3 to 10.0.0
******************************************************************************/

/**************************************
* Update database version
**************************************/
PRINT N'Updating database version ...'

UPDATE [sitecore_commerce_storage].[Versions] SET DBVersion='10.0.0'
GO

PRINT N'Update database version completed.';
GO

PRINT N'Deleting content items and content paths. After running this script you will have to run EnsureSyncDefaultContentPaths from postman'

DELETE FROM [sitecore_commerce_storage].[ContentEntities]
DELETE FROM [sitecore_commerce_storage].[ContentEntity]
DELETE FROM [sitecore_commerce_storage].[ContentLists]
GO

PRINT N'Deleting content items and content paths completed.';
GO

PRINT N'Updating carts stored procedures';
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[PurgeCarts]
GO
CREATE PROCEDURE [sitecore_commerce_storage].[PurgeCarts]
(
	@TableName NVARCHAR(150) = 'CommerceEntity',
    @ArtifactStoreId UNIQUEIDENTIFIER,
    @AbandonedCartsThreshold INT,
	@EmptyCartsThreshold INT
)
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON

    DECLARE @today DATETIME = GETUTCDATE()

	IF (@TableName = '')
		SET @TableName = 'CommerceEntities'

	IF (@TableName = 'CommerceEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpCommerceEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[CommerceEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountCommerceEntity int = 0, @uniqueidCommerceEntity UNIQUEIDENTIFIER, @idCommerceEntity NVARCHAR(150), @dateUpdatedCommerceEntity NVARCHAR(100), @linesCommerceEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpCommerceEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidCommerceEntity, @idCommerceEntity, @dateUpdatedCommerceEntity, @linesCommerceEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceCommerceEntity INT = DATEDIFF(day, @dateUpdatedCommerceEntity, @today)

	        IF ((@differenceCommerceEntity >= @AbandonedCartsThreshold) OR (@linesCommerceEntity = 1 AND @differenceCommerceEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[CommerceEntities] WHERE [UniqueId] = @uniqueidCommerceEntity
		        DELETE FROM [sitecore_commerce_storage].[CommerceEntity] WHERE [UniqueId] = @uniqueidCommerceEntity
                DELETE FROM [sitecore_commerce_storage].[CommerceLists] WHERE [Id] = @idCommerceEntity
            
                SET @deletedCountCommerceEntity = @deletedCountCommerceEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidCommerceEntity, @idCommerceEntity, @dateUpdatedCommerceEntity, @linesCommerceEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpCommerceEntities

        SELECT @deletedCountCommerceEntity
    END
	ELSE IF (@TableName = 'CatalogEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpCatalogEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[CatalogEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountCatalogEntity int = 0, @uniqueidCatalogEntity UNIQUEIDENTIFIER, @idCatalogEntity NVARCHAR(150), @dateUpdatedCatalogEntity NVARCHAR(100), @linesCatalogEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpCatalogEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidCatalogEntity, @idCatalogEntity, @dateUpdatedCatalogEntity, @linesCatalogEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceCatalogEntity INT = DATEDIFF(day, @dateUpdatedCatalogEntity, @today)

	        IF ((@differenceCatalogEntity >= @AbandonedCartsThreshold) OR (@linesCatalogEntity = 1 AND @differenceCatalogEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[CatalogEntities] WHERE [UniqueId] = @uniqueidCatalogEntity
		        DELETE FROM [sitecore_commerce_storage].[CatalogEntity] WHERE [UniqueId] = @uniqueidCatalogEntity
                DELETE FROM [sitecore_commerce_storage].[CatalogLists] WHERE [Id] = @idCatalogEntity
            
                SET @deletedCountCatalogEntity = @deletedCountCatalogEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidCatalogEntity, @idCatalogEntity, @dateUpdatedCatalogEntity, @linesCatalogEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpCatalogEntities

        SELECT @deletedCountCatalogEntity
    END
	ELSE IF (@TableName = 'ContentEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpContentEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[ContentEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountContentEntity int = 0, @uniqueidContentEntity UNIQUEIDENTIFIER, @idContentEntity NVARCHAR(150), @dateUpdatedContentEntity NVARCHAR(100), @linesContentEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpContentEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidContentEntity, @idContentEntity, @dateUpdatedContentEntity, @linesContentEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceContentEntity INT = DATEDIFF(day, @dateUpdatedContentEntity, @today)

	        IF ((@differenceContentEntity >= @AbandonedCartsThreshold) OR (@linesContentEntity = 1 AND @differenceContentEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[ContentEntities] WHERE [UniqueId] = @uniqueidContentEntity
		        DELETE FROM [sitecore_commerce_storage].[ContentEntity] WHERE [UniqueId] = @uniqueidContentEntity
                DELETE FROM [sitecore_commerce_storage].[ContentLists] WHERE [Id] = @idContentEntity
            
                SET @deletedCountContentEntity = @deletedCountContentEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidContentEntity, @idContentEntity, @dateUpdatedContentEntity, @linesContentEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpContentEntities

        SELECT @deletedCountContentEntity
    END
	ELSE IF (@TableName = 'CustomersEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpCustomersEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[CustomersEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountCustomersEntity int = 0, @uniqueidCustomersEntity UNIQUEIDENTIFIER, @idCustomersEntity NVARCHAR(150), @dateUpdatedCustomersEntity NVARCHAR(100), @linesCustomersEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpCustomersEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidCustomersEntity, @idCustomersEntity, @dateUpdatedCustomersEntity, @linesCustomersEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceCustomersEntity INT = DATEDIFF(day, @dateUpdatedCustomersEntity, @today)

	        IF ((@differenceCustomersEntity >= @AbandonedCartsThreshold) OR (@linesCustomersEntity = 1 AND @differenceCustomersEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[CustomersEntities] WHERE [UniqueId] = @uniqueidCustomersEntity
		        DELETE FROM [sitecore_commerce_storage].[CustomersEntity] WHERE [UniqueId] = @uniqueidCustomersEntity
                DELETE FROM [sitecore_commerce_storage].[CustomersLists] WHERE [Id] = @idCustomersEntity
            
                SET @deletedCountCustomersEntity = @deletedCountCustomersEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidCustomersEntity, @idCustomersEntity, @dateUpdatedCustomersEntity, @linesCustomersEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpCustomersEntities

        SELECT @deletedCountCustomersEntity
    END
	ELSE IF (@TableName = 'InventoryEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpInventoryEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[InventoryEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountInventoryEntity int = 0, @uniqueidInventoryEntity UNIQUEIDENTIFIER, @idInventoryEntity NVARCHAR(150), @dateUpdatedInventoryEntity NVARCHAR(100), @linesInventoryEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpInventoryEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidInventoryEntity, @idInventoryEntity, @dateUpdatedInventoryEntity, @linesInventoryEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceInventoryEntity INT = DATEDIFF(day, @dateUpdatedInventoryEntity, @today)

	        IF ((@differenceInventoryEntity >= @AbandonedCartsThreshold) OR (@linesInventoryEntity = 1 AND @differenceInventoryEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[InventoryEntities] WHERE [UniqueId] = @uniqueidInventoryEntity
		        DELETE FROM [sitecore_commerce_storage].[InventoryEntity] WHERE [UniqueId] = @uniqueidInventoryEntity
                DELETE FROM [sitecore_commerce_storage].[InventoryLists] WHERE [Id] = @idInventoryEntity
            
                SET @deletedCountInventoryEntity = @deletedCountInventoryEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidInventoryEntity, @idInventoryEntity, @dateUpdatedInventoryEntity, @linesInventoryEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpInventoryEntities

        SELECT @deletedCountInventoryEntity
    END
	ELSE IF (@TableName = 'LocalizationEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpLocalizationEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[LocalizationEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountLocalizationEntity int = 0, @uniqueidLocalizationEntity UNIQUEIDENTIFIER, @idLocalizationEntity NVARCHAR(150), @dateUpdatedLocalizationEntity NVARCHAR(100), @linesLocalizationEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpLocalizationEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidLocalizationEntity, @idLocalizationEntity, @dateUpdatedLocalizationEntity, @linesLocalizationEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceLocalizationEntity INT = DATEDIFF(day, @dateUpdatedLocalizationEntity, @today)

	        IF ((@differenceLocalizationEntity >= @AbandonedCartsThreshold) OR (@linesLocalizationEntity = 1 AND @differenceLocalizationEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[LocalizationEntities] WHERE [UniqueId] = @uniqueidLocalizationEntity
		        DELETE FROM [sitecore_commerce_storage].[LocalizationEntity] WHERE [UniqueId] = @uniqueidLocalizationEntity
            
                SET @deletedCountLocalizationEntity = @deletedCountLocalizationEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidLocalizationEntity, @idLocalizationEntity, @dateUpdatedLocalizationEntity, @linesLocalizationEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpLocalizationEntities

        SELECT @deletedCountLocalizationEntity
    END
	ELSE IF (@TableName = 'OrdersEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpOrdersEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[OrdersEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountOrdersEntity int = 0, @uniqueidOrdersEntity UNIQUEIDENTIFIER, @idOrdersEntity NVARCHAR(150), @dateUpdatedOrdersEntity NVARCHAR(100), @linesOrdersEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpOrdersEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidOrdersEntity, @idOrdersEntity, @dateUpdatedOrdersEntity, @linesOrdersEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceOrdersEntity INT = DATEDIFF(day, @dateUpdatedOrdersEntity, @today)

	        IF ((@differenceOrdersEntity >= @AbandonedCartsThreshold) OR (@linesOrdersEntity = 1 AND @differenceOrdersEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[OrdersEntities] WHERE [UniqueId] = @uniqueidOrdersEntity
		        DELETE FROM [sitecore_commerce_storage].[OrdersEntity] WHERE [UniqueId] = @uniqueidOrdersEntity
                DELETE FROM [sitecore_commerce_storage].[OrdersLists] WHERE [Id] = @idOrdersEntity
            
                SET @deletedCountOrdersEntity = @deletedCountOrdersEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidOrdersEntity, @idOrdersEntity, @dateUpdatedOrdersEntity, @linesOrdersEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpOrdersEntities

        SELECT @deletedCountOrdersEntity
    END
	ELSE IF (@TableName = 'PricingEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpPricingEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[PricingEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountPricingEntity int = 0, @uniqueidPricingEntity UNIQUEIDENTIFIER, @idPricingEntity NVARCHAR(150), @dateUpdatedPricingEntity NVARCHAR(100), @linesPricingEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpPricingEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidPricingEntity, @idPricingEntity, @dateUpdatedPricingEntity, @linesPricingEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differencePricingEntity INT = DATEDIFF(day, @dateUpdatedPricingEntity, @today)

	        IF ((@differencePricingEntity >= @AbandonedCartsThreshold) OR (@linesPricingEntity = 1 AND @differencePricingEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[PricingEntities] WHERE [UniqueId] = @uniqueidPricingEntity
		        DELETE FROM [sitecore_commerce_storage].[PricingEntity] WHERE [UniqueId] = @uniqueidPricingEntity
                DELETE FROM [sitecore_commerce_storage].[PricingLists] WHERE [Id] = @idPricingEntity
            
                SET @deletedCountPricingEntity = @deletedCountPricingEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidPricingEntity, @idPricingEntity, @dateUpdatedPricingEntity, @linesPricingEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpPricingEntities

        SELECT @deletedCountPricingEntity
    END
	ELSE IF (@TableName = 'PromotionsEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpPromotionsEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[PromotionsEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountPromotionsEntity int = 0, @uniqueidPromotionsEntity UNIQUEIDENTIFIER, @idPromotionsEntity NVARCHAR(150), @dateUpdatedPromotionsEntity NVARCHAR(100), @linesPromotionsEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpPromotionsEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidPromotionsEntity, @idPromotionsEntity, @dateUpdatedPromotionsEntity, @linesPromotionsEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differencePromotionsEntity INT = DATEDIFF(day, @dateUpdatedPromotionsEntity, @today)

	        IF ((@differencePromotionsEntity >= @AbandonedCartsThreshold) OR (@linesPromotionsEntity = 1 AND @differencePromotionsEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[PromotionsEntities] WHERE [UniqueId] = @uniqueidPromotionsEntity
		        DELETE FROM [sitecore_commerce_storage].[PromotionsEntity] WHERE [UniqueId] = @uniqueidPromotionsEntity
                DELETE FROM [sitecore_commerce_storage].[PromotionsLists] WHERE [Id] = @idPromotionsEntity
            
                SET @deletedCountPromotionsEntity = @deletedCountPromotionsEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidPromotionsEntity, @idPromotionsEntity, @dateUpdatedPromotionsEntity, @linesPromotionsEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpPromotionsEntities

        SELECT @deletedCountPromotionsEntity
    END
	ELSE IF (@TableName = 'RelationshipDefinitionEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpRelationshipDefinitionEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[RelationshipDefinitionEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountRelationshipDefinitionEntity int = 0, @uniqueidRelationshipDefinitionEntity UNIQUEIDENTIFIER, @idRelationshipDefinitionEntity NVARCHAR(150), @dateUpdatedRelationshipDefinitionEntity NVARCHAR(100), @linesRelationshipDefinitionEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpRelationshipDefinitionEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidRelationshipDefinitionEntity, @idRelationshipDefinitionEntity, @dateUpdatedRelationshipDefinitionEntity, @linesRelationshipDefinitionEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceRelationshipDefinitionEntity INT = DATEDIFF(day, @dateUpdatedRelationshipDefinitionEntity, @today)

	        IF ((@differenceRelationshipDefinitionEntity >= @AbandonedCartsThreshold) OR (@linesRelationshipDefinitionEntity = 1 AND @differenceRelationshipDefinitionEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[RelationshipDefinitionEntities] WHERE [UniqueId] = @uniqueidRelationshipDefinitionEntity
		        DELETE FROM [sitecore_commerce_storage].[RelationshipDefinitionEntity] WHERE [UniqueId] = @uniqueidRelationshipDefinitionEntity
                DELETE FROM [sitecore_commerce_storage].[RelationshipDefinitionLists] WHERE [Id] = @idRelationshipDefinitionEntity
            
                SET @deletedCountRelationshipDefinitionEntity = @deletedCountRelationshipDefinitionEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidRelationshipDefinitionEntity, @idRelationshipDefinitionEntity, @dateUpdatedRelationshipDefinitionEntity, @linesRelationshipDefinitionEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpRelationshipDefinitionEntities

        SELECT @deletedCountRelationshipDefinitionEntity
    END
	ELSE IF (@TableName = 'VersioningEntities')
    BEGIN
        SELECT 
            [entities].[UniqueId],
            JSON_VALUE([entities].[Entity], '$.Id') AS Id,
	        JSON_VALUE([entities].[Entity], '$.DateUpdated') AS DateUpdated,
	        IIF(JSON_QUERY([entities].[Entity], '$.Lines') IS NULL, 1, 0) AS Lines
        INTO 
            #tmpVersioningEntities
        FROM (
            SELECT [UniqueId], [Entity]
	        FROM [sitecore_commerce_storage].[VersioningEntity]
	        WHERE [ArtifactStoreId] = @ArtifactStoreId 
		        AND JSON_VALUE([Entity], '$."$type"') = 'Sitecore.Commerce.Plugin.Carts.Cart, Sitecore.Commerce.Plugin.Carts'
        ) [entities]

        DECLARE @deletedCountVersioningEntity int = 0, @uniqueidVersioningEntity UNIQUEIDENTIFIER, @idVersioningEntity NVARCHAR(150), @dateUpdatedVersioningEntity NVARCHAR(100), @linesVersioningEntity BIT
        DECLARE carts_cursor CURSOR FOR   
        SELECT [UniqueId], [Id], [DateUpdated], [Lines] FROM #tmpVersioningEntities
  
        OPEN carts_cursor  
  
        FETCH NEXT FROM carts_cursor   
        INTO @uniqueidVersioningEntity, @idVersioningEntity, @dateUpdatedVersioningEntity, @linesVersioningEntity 
  
        WHILE @@FETCH_STATUS = 0  
        BEGIN
	        DECLARE @differenceVersioningEntity INT = DATEDIFF(day, @dateUpdatedVersioningEntity, @today)

	        IF ((@differenceVersioningEntity >= @AbandonedCartsThreshold) OR (@linesVersioningEntity = 1 AND @differenceVersioningEntity >= @EmptyCartsThreshold))
            BEGIN
		        DELETE FROM [sitecore_commerce_storage].[VersioningEntities] WHERE [UniqueId] = @uniqueidVersioningEntity
		        DELETE FROM [sitecore_commerce_storage].[VersioningEntity] WHERE [UniqueId] = @uniqueidVersioningEntity
            
                SET @deletedCountVersioningEntity = @deletedCountVersioningEntity + 1
	        END

	        FETCH NEXT FROM carts_cursor   
            INTO @uniqueidVersioningEntity, @idVersioningEntity, @dateUpdatedVersioningEntity, @linesVersioningEntity  
        END   
        CLOSE carts_cursor
        DEALLOCATE carts_cursor

        DROP TABLE #tmpVersioningEntities

        SELECT @deletedCountVersioningEntity
    END
END
GO

PRINT N'Updating carts stored procedures completed';
GO

PRINT N'Updating lists stored procedures';
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[SelectListEntityMetadataByRange]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[SelectListEntityMetadataByRange]
(
	@ListName nvarchar(150),
	@TableName nvarchar(150) = 'CommerceLists',
	@ArtifactStoreId UNIQUEIDENTIFIER,
	@Skip int = 0,
	@Take int = 1,
	@SortOrder int = 0,
	@IgnorePublished bit = 0
)
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT

	IF (@TableName = '')
		SET @TableName = 'CommerceLists'

	IF (@TableName = 'CommerceLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpCommerceLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[CommerceLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[CommerceEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpCommerceLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpCommerceLists

        DROP TABLE #tmpCommerceLists
	END
	ELSE IF (@TableName = 'CatalogLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpCatalogLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[CatalogLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[CatalogEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpCatalogLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpCatalogLists

        DROP TABLE #tmpCatalogLists
	END
	ELSE IF (@TableName = 'ContentLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpContentLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[ContentLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[ContentEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpContentLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpContentLists

        DROP TABLE #tmpContentLists
	END
	ELSE IF (@TableName = 'CustomersLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpCustomersLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[CustomersLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[CustomersEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpCustomersLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpCustomersLists

        DROP TABLE #tmpCustomersLists
	END
	ELSE IF (@TableName = 'InventoryLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpInventoryLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[InventoryLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[InventoryEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpInventoryLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpInventoryLists

        DROP TABLE #tmpInventoryLists
	END
	ELSE IF (@TableName = 'OrdersLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpOrdersLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[OrdersLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[OrdersEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpOrdersLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpOrdersLists

        DROP TABLE #tmpOrdersLists
	END
	ELSE IF (@TableName = 'PricingLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpPricingLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[PricingLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[PricingEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpPricingLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpPricingLists

        DROP TABLE #tmpPricingLists
	END
	ELSE IF (@TableName = 'PromotionsLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpPromotionsLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[PromotionsLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[PromotionsEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpPromotionsLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpPromotionsLists

        DROP TABLE #tmpPromotionsLists
	END
	ELSE IF (@TableName = 'RelationshipDefinitionLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpRelationshipDefinitionLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[RelationshipDefinitionLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[RelationshipDefinitionEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpRelationshipDefinitionLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpRelationshipDefinitionLists

        DROP TABLE #tmpRelationshipDefinitionLists
	END
	ELSE IF (@TableName = 'RelationshipLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpRelationshipLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[RelationshipLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[CatalogEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId]
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpRelationshipLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpRelationshipLists

        DROP TABLE #tmpRelationshipLists
	END
	ELSE IF (@TableName = 'WorkflowLists')
	BEGIN
		-- Get matching entities
		SELECT 
			[entities].[UniqueId],
			[entities].[Id],
			[entities].[EntityVersion]
		INTO 
			#tmpWorkflowLists
		FROM (
			SELECT
				[innerEntities].[UniqueId],
				[lists].[Id],
				[innerEntities].[EntityVersion],
				ROW_NUMBER() OVER(PARTITION BY ISNULL([innerEntities].[Id], [lists].[Id]) ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
			FROM
				[sitecore_commerce_storage].[WorkflowLists] [lists]
			LEFT JOIN 
				[sitecore_commerce_storage].[CatalogEntities] [innerEntities] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = [innerEntities].[ArtifactStoreId] AND [lists].[EntityVersion] = [innerEntities].[EntityVersion] -- NOTE: WorkflowLists needs to compare the entity version
			WHERE 
				[lists].[ListName] = @ListName AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1 OR [innerEntities].[UniqueId] IS NULL)
		) [entities]
		WHERE rowNumber = 1

		---- First result: Paged results
		SELECT 
			[entities].[Id],
			[entities].[UniqueId],
			[entities].[EntityVersion]
		FROM 
			#tmpWorkflowLists entities
		ORDER BY
			CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
			CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY

		-- Second result: Total count
		SELECT 
			COUNT([Id]) AS TotalCount
		FROM 
			#tmpWorkflowLists

        DROP TABLE #tmpWorkflowLists
	END
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[SelectListEntitiesByRange]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[SelectListEntitiesByRange]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @ArtifactStoreId UNIQUEIDENTIFIER,
    @Skip int = 0,
    @Take int = 1,
    @SortOrder int = 0,
    @IgnorePublished bit = 0
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT

    IF (@TableName = '')
        SET @TableName = 'CommerceLists'

    IF (@TableName = 'CommerceLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCommerceLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CommerceEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CommerceLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCommerceLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CommerceEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCommerceLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CommerceEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CommerceLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'CatalogLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCatalogLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CatalogLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCatalogLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCatalogLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CatalogLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'ContentLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpContentLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[ContentEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[ContentLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpContentLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[ContentEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpContentLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[ContentEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[ContentLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'CustomersLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCustomersLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CustomersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CustomersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCustomersLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CustomersEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCustomersLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CustomersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CustomersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'InventoryLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpInventoryLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[InventoryEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[InventoryLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpInventoryLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[InventoryEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpInventoryLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[InventoryEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[InventoryLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'OrdersLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpOrdersLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[OrdersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[OrdersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpOrdersLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[OrdersEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpOrdersLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[OrdersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[OrdersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'PricingLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpPricingLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PricingEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PricingLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpPricingLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[PricingEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpPricingLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PricingEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PricingLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'PromotionsLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpPromotionsLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PromotionsEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PromotionsLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpPromotionsLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[PromotionsEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpPromotionsLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PromotionsEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PromotionsLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'RelationshipDefinitionLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpRelationshipDefinitionLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[RelationshipDefinitionEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipDefinitionLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpRelationshipDefinitionLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[RelationshipDefinitionEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpRelationshipDefinitionLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[RelationshipDefinitionEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipDefinitionLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'RelationshipLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpRelationshipLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpRelationshipLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpRelationshipLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'WorkflowLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpWorkflowLists
        FROM (
            SELECT
                [innerEntities].[UniqueId],
                [innerEntities].[Id],
                [innerEntities].[EntityVersion],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[WorkflowLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND [lists].[EntityVersion] = [innerEntities].[EntityVersion] -- NOTE: WorkflowLists needs to compare the entity version
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpWorkflowLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpWorkflowLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[WorkflowLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId AND [lists].[EntityVersion] = [innerEntities].[EntityVersion] -- NOTE: WorkflowLists needs to compare the entity version
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId AND (@IgnorePublished = 1 OR [innerEntities].[Published] = 1)
        ) [entities]
        WHERE rowNumber = 1
    END
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[SelectListEntitiesByRangeWithoutVersioning]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[SelectListEntitiesByRangeWithoutVersioning]
(
    @ListName nvarchar(150),
    @TableName nvarchar(150) = 'CommerceLists',
    @ArtifactStoreId UNIQUEIDENTIFIER,
    @Skip int = 0,
    @Take int = 1,
    @SortOrder int = 0,
    @IgnorePublished bit = 0
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT

    IF (@TableName = '')
        SET @TableName = 'CommerceLists'

    IF (@TableName = 'CommerceLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCommerceLists
        FROM 
            [sitecore_commerce_storage].[CommerceEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[CommerceLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCommerceLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CommerceEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCommerceLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CommerceEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CommerceLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'CatalogLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCatalogLists
        FROM 
            [sitecore_commerce_storage].[CatalogEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[CatalogLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCatalogLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCatalogLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CatalogLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'ContentLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpContentLists
        FROM 
            [sitecore_commerce_storage].[ContentEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[ContentLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpContentLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[ContentEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpContentLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[ContentEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[ContentLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'CustomersLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpCustomersLists
        FROM 
            [sitecore_commerce_storage].[CustomersEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[CustomersLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpCustomersLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CustomersEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpCustomersLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CustomersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[CustomersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'InventoryLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpInventoryLists
        FROM 
            [sitecore_commerce_storage].[InventoryEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[InventoryLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpInventoryLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[InventoryEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpInventoryLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[InventoryEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[InventoryLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'OrdersLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpOrdersLists
        FROM 
            [sitecore_commerce_storage].[OrdersEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[OrdersLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpOrdersLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[OrdersEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpOrdersLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[OrdersEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[OrdersLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'PricingLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpPricingLists
        FROM 
            [sitecore_commerce_storage].[PricingEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[PricingLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpPricingLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[PricingEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpPricingLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PricingEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PricingLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'PromotionsLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpPromotionsLists
        FROM 
            [sitecore_commerce_storage].[PromotionsEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[PromotionsLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpPromotionsLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[PromotionsEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpPromotionsLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[PromotionsEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[PromotionsLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'RelationshipDefinitionLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpRelationshipDefinitionLists
        FROM 
            [sitecore_commerce_storage].[RelationshipDefinitionEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[RelationshipDefinitionLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpRelationshipDefinitionLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[RelationshipDefinitionEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpRelationshipDefinitionLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[RelationshipDefinitionEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipDefinitionLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'RelationshipLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpRelationshipLists
        FROM 
            [sitecore_commerce_storage].[CatalogEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[RelationshipLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpRelationshipLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpRelationshipLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[RelationshipLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
    ELSE IF (@TableName = 'WorkflowLists')
    BEGIN
        -- Get matching entities
        SELECT 
            [entities].[UniqueId],
            [entities].[Id],
            [entities].[EntityVersion]
        INTO 
            #tmpWorkflowLists
        FROM 
            [sitecore_commerce_storage].[CatalogEntities] [entities]
        INNER JOIN 
            [sitecore_commerce_storage].[WorkflowLists] [lists] ON [lists].[Id] = [entities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
        WHERE
            [lists].[ListName] = @ListName AND [entities].[ArtifactStoreId] = @ArtifactStoreId
        ORDER BY
            [entities].[Id]
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY

        -- First result: Paged results
        SELECT 
            [entities].[Id],
            [entities].[UniqueId],
            [entities].[EntityVersion],
            [entity].[Entity] AS [Entity],
            [localization].[Entity] AS [LocalizationEntity]
        FROM 
            #tmpWorkflowLists [entities]
        INNER JOIN
            [sitecore_commerce_storage].[CatalogEntity] [entity] ON [entity].[UniqueId] = [entities].[UniqueId]
        LEFT OUTER JOIN
            [sitecore_commerce_storage].[LocalizationEntity] [localization] ON [entities].[UniqueId] = [localization].[EntityUniqueId]
        ORDER BY
            CASE WHEN @SortOrder = 0 THEN [entities].[Id] END ASC,
            CASE WHEN @SortOrder = 1 THEN [entities].[Id] END DESC

        DROP TABLE #tmpWorkflowLists

        -- Second result: Total count
        SELECT
            COUNT([entities].[Id]) AS [TotalCount]
        FROM (
            SELECT
                [innerEntities].[Id],
                ROW_NUMBER() OVER(PARTITION BY [innerEntities].[Id] ORDER BY [innerEntities].[EntityVersion] DESC) rowNumber
            FROM
                [sitecore_commerce_storage].[CatalogEntities] [innerEntities]
            INNER JOIN 
                [sitecore_commerce_storage].[WorkflowLists] [lists] ON [lists].[Id] = [innerEntities].[Id] AND [lists].[ArtifactStoreId] = @ArtifactStoreId
            WHERE 
                [lists].[ListName] = @ListName AND [innerEntities].[ArtifactStoreId] = @ArtifactStoreId
        ) [entities]
        WHERE rowNumber = 1
    END
END
GO

PRINT N'Updating lists stored procedures completed.';
GO

PRINT N'Updating mappings stored procedures';
GO

DROP INDEX IF EXISTS [IX_Mappings_VariationId_ParentCatalog] ON [sitecore_commerce_storage].[Mappings]
GO

CREATE NONCLUSTERED INDEX [IX_Mappings_VariationId_ParentCatalog]
    ON [sitecore_commerce_storage].[Mappings]
(
    [VariationId],
    [ParentCatalog]
) INCLUDE ([EntityId], [EntityVersion])

DROP INDEX IF EXISTS [IX_Mappings_VariationId] ON [sitecore_commerce_storage].[Mappings]
GO

CREATE NONCLUSTERED INDEX [IX_Mappings_VariationId]
	ON [sitecore_commerce_storage].[Mappings]
(
	[VariationId] ASC
)

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogGetMappings]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogGetMappings]
(
    @ArtifactStoreId UNIQUEIDENTIFIER
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DECLARE @CatalogIds TABLE([CatalogDeterministicId] UNIQUEIDENTIFIER)

    -- Find mapped catalogs
    INSERT INTO
        @CatalogIds
    SELECT
        [DeterministicId]
    FROM
        [sitecore_commerce_storage].[Mappings]
    WITH (NOLOCK)
    WHERE
        [EntityId] LIKE 'Entity-Catalog-%' AND [ParentId] IS NOT NULL AND [ArtifactStoreId] = @ArtifactStoreId

    -- Filter mapping entries
    SELECT
        DISTINCT [EntityId]
        ,[EntityVersion]
        ,[Published]
        ,[VariationId]
        ,[SitecoreId]
        ,[DeterministicId]
        ,[ParentId]
        ,[IsBundle]
        ,[ParentCatalog]
    FROM
        [sitecore_commerce_storage].[Mappings]
    WITH (NOLOCK)
    INNER JOIN @CatalogIds [ids]
        ON [DeterministicId] = [ids].[CatalogDeterministicId] OR [ParentCatalog] = [ids].[CatalogDeterministicId]
    WHERE
        [ArtifactStoreId] = @ArtifactStoreId
END

GO


DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogGetMappingsMaster]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogGetMappingsMaster]
(
    @ArtifactStoreId UNIQUEIDENTIFIER,
    @Skip INT = 0,
    @Take INT = 1000
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON

    -- Get catalogs
    SELECT
        [Id],
        [SitecoreId],
        [EntityId],
        [EntityVersion]
    INTO
        #tmpCatalogs
    FROM
    (
        SELECT
            [Id],
            CONVERT(NVARCHAR(150), [SitecoreId]) AS [SitecoreId],
            [EntityId],
            [EntityVersion],
            ROW_NUMBER() OVER(PARTITION BY [mappings].[EntityId] ORDER BY [mappings].[EntityVersion] DESC) [rowNumber]
        FROM
            [sitecore_commerce_storage].[Mappings]
        WITH (NOLOCK)
        WHERE
            [EntityId] LIKE 'Entity-Catalog-%' AND [ParentId] IS NOT NULL AND [ArtifactStoreId] = @ArtifactStoreId
    ) [inner]
    WHERE [inner].[rowNumber] = 1

    -- Get matching entries
    SELECT
        [EntityId],
        [EntityVersion]
    INTO
        #tmpEntries
    FROM
    (
        SELECT
            [mappings].[EntityId],
            [mappings].[EntityVersion],
            ROW_NUMBER() OVER(PARTITION BY [mappings].[EntityId] ORDER BY [mappings].[EntityVersion] DESC) [rowNumber]
        FROM
            [sitecore_commerce_storage].[Mappings] [mappings]
        WITH (NOLOCK)
        INNER JOIN
            #tmpCatalogs [catalog] ON [mappings].[ParentCatalog] = [catalog].[SitecoreId] OR [mappings].[Id] = [catalog].[Id]
        WHERE
            [VariationId] IS NULL
    ) [inner]
    WHERE [inner].[rowNumber] = 1
    ORDER BY [EntityId] ASC
    OFFSET @Skip ROWS
    FETCH NEXT @Take ROWS ONLY

    -- Get results
    SELECT
        [EntityId],
        [EntityVersion]
    FROM
        #tmpEntries
    ORDER BY [EntityId] ASC
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogGetMappingsWeb]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogGetMappingsWeb]
(
    @ArtifactStoreId UNIQUEIDENTIFIER,
    @Skip INT = 0,
    @Take INT = 1000
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON

    -- Get catalogs
    SELECT
        [Id],
        [SitecoreId],
        [EntityId],
        [EntityVersion]
    INTO
        #tmpCatalogs
    FROM
    (
        SELECT
            [Id],
            CONVERT(NVARCHAR(150), [SitecoreId]) AS [SitecoreId],
            [EntityId],
            [EntityVersion],
            ROW_NUMBER() OVER(PARTITION BY [mappings].[EntityId] ORDER BY [mappings].[EntityVersion] DESC) [rowNumber]
        FROM
            [sitecore_commerce_storage].[Mappings]
        WITH (NOLOCK)
        WHERE
            [EntityId] LIKE 'Entity-Catalog-%' AND [ParentId] IS NOT NULL AND [Published] = 1 AND [ArtifactStoreId] = @ArtifactStoreId
    ) [inner]
    WHERE [inner].[rowNumber] = 1

    -- Get matching entries
    SELECT
        [EntityId],
        [EntityVersion]
    INTO
        #tmpEntries
    FROM
    (
        SELECT
            [mappings].[EntityId],
            [mappings].[EntityVersion],
            ROW_NUMBER() OVER(PARTITION BY [mappings].[EntityId] ORDER BY [mappings].[EntityVersion] DESC) [rowNumber]
        FROM
            [sitecore_commerce_storage].[Mappings] [mappings]
        WITH (NOLOCK)
        INNER JOIN
            #tmpCatalogs [catalog] ON [mappings].[ParentCatalog] = [catalog].[SitecoreId] OR [mappings].[Id] = [catalog].[Id]
        WHERE
            [VariationId] IS NULL AND [Published] = 1
    ) [inner]
    WHERE [inner].[rowNumber] = 1
    ORDER BY [EntityId] ASC
    OFFSET @Skip ROWS
    FETCH NEXT @Take ROWS ONLY

    -- Get results
    SELECT
        [EntityId],
        [EntityVersion]
    FROM
        #tmpEntries
    ORDER BY [EntityId] ASC
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogUpdateMappings]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogUpdateMappings]
(
	@Id NVARCHAR(150),
	@EntityVersion INT,
	@Published BIT,
	@ArtifactStoreId UNIQUEIDENTIFIER,
	@SitecoreId UNIQUEIDENTIFIER,
	@ParentCatalogList NVARCHAR(MAX),
	@CatalogToEntityList NVARCHAR(MAX),
	@ChildrenCategoryList NVARCHAR(MAX),
	@ChildrenSellableItemList NVARCHAR(MAX),
	@ParentCategoryList NVARCHAR(MAX),
	@IsBundle BIT,
	@ItemVariations NVARCHAR(MAX)
)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON

	DELETE FROM 
		[sitecore_commerce_storage].[Mappings]
	WHERE 
		ArtifactStoreId = @ArtifactStoreId AND EntityId = @Id AND EntityVersion = @EntityVersion

	DECLARE @CatalogMappings TABLE 
	(
		Id NVARCHAR(150),
		EntityVersion INT,
		Published BIT,
		ArtifactStoreId UNIQUEIDENTIFIER NOT NULL,
		SitecoreId UNIQUEIDENTIFIER,
		ParentCatalogList NVARCHAR(MAX) NULL,
		CatalogToEntityList NVARCHAR(MAX) NULL,
		ChildrenCategoryList NVARCHAR(MAX) NULL,
		ChildrenSellableItemList NVARCHAR(MAX) NULL,
		ParentCategoryList NVARCHAR(MAX) NULL,
		IsBundle BIT NULL,
		ItemVariations NVARCHAR(MAX) NULL
	)

	INSERT INTO
		@CatalogMappings 
	SELECT 
		@Id, 
		@EntityVersion, 
		@Published, 
		@ArtifactStoreId, 
		@SitecoreId, 
		@ParentCatalogList, 
		@CatalogToEntityList, 
		@ChildrenCategoryList, 
		@ChildrenSellableItemList, 
		@ParentCategoryList,
		@IsBundle, 
		@ItemVariations

	IF(@Id LIKE 'Entity-Catalog-%')
	BEGIN
		INSERT INTO 
			[sitecore_commerce_storage].[Mappings]
		SELECT DISTINCT
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,@SitecoreId AS DeterministicId
			,IIF(LEN(@ParentCatalogList) > 0, @ParentCatalogList, NULL) AS ParentId
			,NULL AS [IsBundle]
            ,NULL AS ParentCatalog
		FROM 
			@CatalogMappings
	END
	ELSE IF(@Id LIKE 'Entity-Category-%')
	BEGIN
		INSERT INTO	
			[sitecore_commerce_storage].[Mappings]
		SELECT DISTINCT
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,@SitecoreId AS DeterministicId
			,IIF(LEN(ParentCategory.VALUE) > 0, ParentCategory.VALUE, IIF(LEN(CatalogToEntity.VALUE) > 0, CatalogToEntity.VALUE, NULL)) AS ParentId
			,NULL AS [IsBundle]
            ,ParentCatalog.value AS ParentCatalog
		FROM 
			@CatalogMappings
				OUTER APPLY STRING_SPLIT(CatalogToEntityList, '|') AS CatalogToEntity
				OUTER APPLY STRING_SPLIT(ParentCatalogList, '|') AS ParentCatalog
				OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') AS ParentCategory
	END
	ELSE IF(@Id LIKE 'Entity-SellableItem-%')
	BEGIN
		INSERT INTO 
			[sitecore_commerce_storage].[Mappings]
		SELECT 
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.VALUE)) AS VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(ParentCategory.VALUE) > 0, ParentCategory.VALUE, NULL) AS ParentId
			,@IsBundle AS [IsBundle]
            ,IIF(LEN(ParentCategory.VALUE) > 0, (SELECT TOP 1 [ParentCatalog] FROM [sitecore_commerce_storage].[Mappings] WHERE [DeterministicId] = ParentCategory.VALUE), NULL) AS ParentCatalog
		FROM
			@CatalogMappings
				CROSS APPLY STRING_SPLIT(ParentCategoryList, '|') AS ParentCategory
		
		UNION
		
		SELECT 
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,ItemVariations.VALUE AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.VALUE, '|', LOWER(CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.VALUE)) AS VARCHAR(100))), 2))) AS varchar(200))
				)) 	 AS DeterministicId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', ParentCategory.VALUE)) AS VARCHAR(100))), 2) AS ParentId
			,@IsBundle AS [IsBundle]
            ,IIF(LEN(ParentCategory.VALUE) > 0, (SELECT TOP 1 [ParentCatalog] FROM [sitecore_commerce_storage].[Mappings] WHERE [DeterministicId] = ParentCategory.VALUE), NULL) AS ParentCatalog
		FROM 
			@CatalogMappings
				OUTER APPLY STRING_SPLIT(ParentCategoryList, '|') AS ParentCategory
				CROSS APPLY STRING_SPLIT(ItemVariations, '|') AS ItemVariations
				
		UNION

		SELECT 
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,NULL AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.VALUE)) AS VARCHAR(100))), 2) AS DeterministicId
			,IIF(LEN(CatalogEntities.VALUE) > 0, CatalogEntities.VALUE, NULL) AS ParentId
			,@IsBundle AS [IsBundle]
            ,IIF(LEN(CatalogEntities.VALUE) > 0, (SELECT TOP 1 [DeterministicId] FROM [sitecore_commerce_storage].[Mappings] WHERE [DeterministicId] = CatalogEntities.VALUE), NULL) AS ParentCatalog
		FROM 
			@CatalogMappings
				CROSS APPLY STRING_SPLIT(CatalogToEntityList, '|') AS CatalogEntities
		WHERE 
			LEN(CatalogEntities.VALUE) > 0
		
		UNION
		
		SELECT 
			NEWID() AS Id
			,@Id AS EntityId
			,@EntityVersion AS EntityVersion
			,@Published AS Published
			,@ArtifactStoreId AS ArtifactStoreId
			,ItemVariations.VALUE AS VariationId
			,@SitecoreId AS SitecoreId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', 
				CAST(CONCAT(@Id, '|', ItemVariations.VALUE, '|', LOWER(CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.VALUE)) AS VARCHAR(100))), 2))) AS varchar(200))
				)) 	 AS DeterministicId
			,CONVERT(UNIQUEIDENTIFIER, HashBytes('MD5', CAST(LOWER(CONCAT(@SitecoreId, '|', CatalogEntities.VALUE)) AS VARCHAR(100))), 2) AS ParentId
			,@IsBundle AS [IsBundle]
            ,IIF(LEN(CatalogEntities.VALUE) > 0, (SELECT TOP 1 [DeterministicId] FROM [sitecore_commerce_storage].[Mappings] WHERE [DeterministicId] = CatalogEntities.VALUE), NULL) AS ParentCatalog
		FROM 
			@CatalogMappings
				OUTER APPLY STRING_SPLIT(CatalogToEntityList, '|') AS CatalogEntities
				CROSS APPLY STRING_SPLIT(ItemVariations, '|') AS ItemVariations
		WHERE 
			LEN(CatalogEntities.VALUE) > 0
	END
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogGetHierarchy]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogGetHierarchy]
(
	@EntityId NVARCHAR(150),
	@IgnorePublished BIT = 0
)
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @deterministicId UNIQUEIDENTIFIER

	DECLARE deterministicIdCursor CURSOR FORWARD_ONLY FOR
	SELECT [DeterministicId]
	FROM (
		SELECT
			[DeterministicId],
			ROW_NUMBER() OVER(PARTITION BY [DeterministicId] ORDER BY [EntityVersion] DESC) rowNumber
		FROM
			[sitecore_commerce_storage].[Mappings]
		WHERE
			[EntityId] = @EntityId AND [VariationId] IS NULL AND ([Published] = 1 OR @IgnorePublished = 1)
	) x
	WHERE x.rowNumber = 1

	OPEN deterministicIdCursor

	FETCH NEXT FROM deterministicIdCursor INTO @deterministicId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		;WITH CTE AS (
			SELECT *
			FROM (
				SELECT
					[EntityId],
					[DeterministicId],
					[ParentId],			
					0 AS [Level],
					ROW_NUMBER() OVER(PARTITION BY [DeterministicId] ORDER BY [EntityVersion] DESC) rowNumber
				FROM
					[sitecore_commerce_storage].[Mappings]
				WHERE
					[DeterministicId] = @deterministicId AND ([Published] = 1 OR @IgnorePublished = 1)
			) first
			WHERE first.rowNumber = 1

			UNION ALL

			SELECT
				[m].[EntityId],
				[m].[DeterministicId],
				[m].[ParentId],
				([Level] + 1) AS [Level],
				ROW_NUMBER() OVER(PARTITION BY [m].[DeterministicId] ORDER BY [m].[EntityVersion] DESC) rowNumber			
			FROM
				[sitecore_commerce_storage].[Mappings] m
			INNER JOIN CTE c ON [m].[DeterministicId] = [c].[ParentId]  AND ([m].[Published] = 1 OR @IgnorePublished = 1)
		), CTE2 AS (
			SELECT DISTINCT [EntityId], [DeterministicId], [Level] FROM CTE
		)
		SELECT [EntityId], [DeterministicId] FROM CTE2 ORDER BY [Level] DESC

		FETCH NEXT FROM deterministicIdCursor INTO @deterministicId
	END

	CLOSE deterministicIdCursor
	DEALLOCATE deterministicIdCursor
END
GO

DROP PROCEDURE IF EXISTS [sitecore_commerce_storage].[CatalogGetMappingsForId]
GO

CREATE PROCEDURE [sitecore_commerce_storage].[CatalogGetMappingsForId]
(
	@DeterministicIds [sitecore_commerce_storage].[SitecoreIdList] READONLY
)
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT [SitecoreId] INTO #tmpSitecoreIds FROM [sitecore_commerce_storage].[Mappings] WHERE DeterministicId IN (SELECT [SitecoreId] FROM @DeterministicIds)


	SELECT DISTINCT [EntityId]
	    ,[EntityVersion]
	    ,[Published]
	    ,[VariationId]
	    ,[Mappings].[SitecoreId]
	    ,[DeterministicId]
	    ,[ParentId]
	    ,[IsBundle]
	    ,[ParentCatalog]
    FROM [sitecore_commerce_storage].[Mappings] WITH (NOLOCK)
    WHERE [SitecoreId] IN (SELECT [SitecoreId] FROM #tmpSitecoreIds) AND [ParentId] IS NOT NULL

    UNION 

    SELECT DISTINCT [EntityId]
	    ,[EntityVersion]
	    ,[Published]
	    ,[VariationId]
	    ,[Mappings].[SitecoreId]
	    ,[DeterministicId]
	    ,[ParentId]
	    ,[IsBundle]
	    ,[ParentCatalog]
    FROM [sitecore_commerce_storage].[Mappings] WITH (NOLOCK)
    WHERE [SitecoreId] IN (SELECT [SitecoreId] FROM #tmpSitecoreIds)
    ORDER BY [VariationId] ASC
END
GO

PRINT N'Updating mappings stored procedures completed';
GO