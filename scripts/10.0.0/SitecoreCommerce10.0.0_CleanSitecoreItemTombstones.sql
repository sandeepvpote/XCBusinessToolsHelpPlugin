/******************************************************************************
* This script should run against 'SitecoreCommerce_SharedEnvironments' and
* 'SitecoreCommerce_Global' to upgrade from Sitecore XC 9.3 to 10.0.0
******************************************************************************/

PRINT N'Cleaning SitecoreItemTombstone entities from all Commerce tables'
GO

DELETE FROM [sitecore_commerce_storage].[CommerceEntities]
    WHERE Id LIKE 'Entity-SitecoreItemTombstone-%'
GO

DELETE FROM [sitecore_commerce_storage].[CommerceEntity]
    WHERE JSON_VALUE(Entity, '$.Id') LIKE 'Entity-SitecoreItemTombstone-%'
GO

DELETE FROM [sitecore_commerce_storage].[CommerceLists]
    WHERE Id LIKE 'Entity-SitecoreItemTombstone-%'
GO

PRINT N'Cleaning SitecoreItemTombstone entities completed.'
GO
