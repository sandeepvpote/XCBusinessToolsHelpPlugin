/******************************************************************************
* This script should run against 'SitecoreCommerce_SharedEnvironments' to 
* upgrade from Sitecore XC 9.3 to 10.0.0
******************************************************************************/

/******************************************************************************
* Iterate through all Sellable Item mapping entries where the parent catalog
* is null and set it accordingly.
******************************************************************************/
DECLARE @entityId as nvarchar(150)
DECLARE @deterministicId as uniqueidentifier
DECLARE @parentId as uniqueidentifier

SELECT EntityId, DeterministicId, ParentId INTO #tmpMappings
FROM sitecore_commerce_storage.Mappings
WHERE EntityId like '%Entity-SellableItem-%' and ParentCatalog is null

DECLARE entity_cursor CURSOR FOR   
	SELECT EntityId, DeterministicId, ParentId
		FROM #tmpMappings 

OPEN entity_cursor
FETCH next FROM entity_cursor into @entityId, @deterministicId, @parentId
WHILE @@FETCH_STATUS = 0  
BEGIN  
	UPDATE [sitecore_commerce_storage].[Mappings]
	SET ParentCatalog = (select ParentCatalog from [sitecore_commerce_storage].[Mappings] WHERE DeterministicId = @parentId)
	WHERE DeterministicId = @deterministicId

	FETCH next FROM entity_cursor into @entityId, @deterministicId, @parentId
END

CLOSE entity_cursor;
DEALLOCATE entity_cursor;
DROP TABLE #tmpMappings
