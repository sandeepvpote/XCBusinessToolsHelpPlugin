/******************************************************************************
* This script should run against 'SitecoreCommerce_SharedEnvironments' to upgrade from Sitecore XC 9.3 to 10.0.0
******************************************************************************/

/**************************************
* Update RelationshipLists to remove versioned lists
**************************************/

PRINT N'Removing versioned lists in RelationshipLists'
GO

SELECT DISTINCT ArtifactStoreId Into #tmpArtifactStoreIds
FROM [sitecore_commerce_storage].[RelationshipLists] 
  
  DECLARE @artifactId nvarchar(150)
  DECLARE curs CURSOR FOR SELECT ArtifactStoreId FROM #tmpArtifactStoreIds
  OPEN curs

  FETCH NEXT FROM curs INTO @artifactId

 WHILE @@FETCH_STATUS = 0 
 BEGIN
	
	PRINT N'Removing versioned lists for ArtifactStoreId ' + @artifactId ;
   
	SELECT [ListName],[Id] INTO #tmpCommerceLists
	  FROM [sitecore_commerce_storage].[RelationshipLists] 
	  Where ArtifactStoreId = @artifactId and
	  ListName not like '%BUNDLETOSELLABLEITEM%' and
	  ListName like '%-_-ByDate' 
	  
	  DECLARE @id nvarchar(150)
	  DECLARE @listName nvarchar(150)
	  
	  DECLARE cur CURSOR FOR SELECT [ListName],[Id] FROM #tmpCommerceLists
	  OPEN cur
	  FETCH NEXT FROM cur INTO @listName,@id
	  
	  WHILE @@FETCH_STATUS = 0 
	  BEGIN
		 DECLARE @name nvarchar(150);		
		 SET @name = LEFT(@listName, LEN(@listName)-8);

		 DELETE FROM [sitecore_commerce_storage].[RelationshipLists] WHERE ListName = CONCAT(@name,'ByDate') and Id = @id;

		 UPDATE [sitecore_commerce_storage].[RelationshipLists] SET ListName = CONCAT(@name,'ByDate') WHERE Id = @id and ListName = 
		 (SELECT Top 1 ListName FROM [sitecore_commerce_storage].[RelationshipLists] WHERE Id = @id and ListName like @name+'%' ORDER BY ListName DESC);
		 		 
		 DELETE FROM [sitecore_commerce_storage].[RelationshipLists] WHERE ListName like CONCAT(@name,'%-ByDate') and Id = @id;
		 DELETE FROM #tmpCommerceLists WHERE ListName like CONCAT(@name,'%-ByDate') and Id = @id;	

		 FETCH NEXT FROM cur INTO @listName,@id
	  END

	  CLOSE cur    
	  DEALLOCATE cur
	  DROP TABLE #tmpCommerceLists 
		
  FETCH NEXT FROM curs INTO @artifactId
END
			

CLOSE curs    
DEALLOCATE curs

Drop Table #tmpArtifactStoreIds 

PRINT N'Done with removing versioned lists';