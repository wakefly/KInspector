SELECT S.SiteName, DocumentCulture, Count(*)
FROM View_CMS_Tree_Joined AS V
INNER JOIN CMS_Site AS S on S.SiteID = V.NodeSiteID
WHERE DocumentPageTemplateID = @PageTemplateID 
GROUP BY S.SiteName, DocumentPageTemplateID, DocumentCulture
ORDER BY S.SiteName, DocumentCulture