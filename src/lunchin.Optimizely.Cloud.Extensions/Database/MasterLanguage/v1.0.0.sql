CREATE OR ALTER PROCEDURE [dbo].[lunchin_ChangePageBranchMasterLanguage] 
    @page_id    int, 
    @language_branch varchar(20),
    @recursive bit,
    @switch_only bit 
AS 

BEGIN       
    DECLARE @language_branch_id nchar(17);
    DECLARE @language_branch_nid int;  
    DECLARE @prev_language_branch_nid int;  
    DECLARE @child_Id int;  
    DECLARE @Fetch int;  
    DECLARE @target_lang_version_exist int;  
                        
    SET @language_branch_nid = (SELECT pkID FROM tblLanguageBranch WHERE(LanguageID = @language_branch))  
    SET @language_branch_id = (SELECT LanguageId FROM tblLanguageBranch WHERE(LanguageID = @language_branch))  
    SET @prev_language_branch_nid = (SELECT fkMasterLanguageBranchID FROM tblContent WHERE pkId = @page_id)  
    SET @target_lang_version_exist = (SELECT count(*) FROM tblContentLanguage WHERE(fkContentID = @page_id AND fkLanguageBranchID = @language_branch_nid))  
                        
                        
    IF 1 = @switch_only  
        BEGIN  
   
            IF @target_lang_version_exist > 0  
                        
                BEGIN  
                    UPDATE tblContent  SET fkMasterLanguageBranchID = @language_branch_nid  
                    WHERE pkID = @page_id AND fkMasterLanguageBranchID = @prev_language_branch_nid  
                END  
            ELSE  
                BEGIN  
                    RAISERROR (N'The Selected page with ID:%d, cannot switch master branch since there is no version in the selected target language: %s.  Please use the convert option instead of switch only.',  11, 1, @page_id, @language_branch);  
                END  
        END  
    ELSE  
        BEGIN  
                        
            IF @target_lang_version_exist > 0  
                        
                BEGIN  
                        
                    RAISERROR (N'The Selected page with ID:%d, cannot be translated since there already is a version in the selected target language: %s.  Please use the convert option instead of switch only.',  11, 1, @page_id, @language_branch);  
                END  
            ELSE  
                          
                BEGIN  
                    UPDATE tblContent  SET  fkMasterLanguageBranchID = @language_branch_nid  
                    WHERE pkId = @page_id  
                          
                    UPDATE tblContentProperty  
                    SET fkLanguageBranchID = @language_branch_nid  
                    WHERE fkContentID = @page_id AND fkLanguageBranchID = @prev_language_branch_nid  
                          
                    UPDATE tblContentLanguage  
                    SET fkLanguageBranchID = @language_branch_nid  
                    WHERE fkContentID = @page_id AND fkLanguageBranchID = @prev_language_branch_nid  
                          
                          
                    UPDATE tblWorkContent  
                    SET fkLanguageBranchID = @language_branch_nid  
                    WHERE fkContentID = @page_id AND fkLanguageBranchID = @prev_language_branch_nid  
                END  
        END  
                          
    IF 1 = @recursive  
        BEGIN  
        
            DECLARE children_cursor CURSOR LOCAL FOR  
                          
            SELECT pkId from tblContent WHERE fkParentId = @page_id  
                          
            OPEN children_cursor  
                          
            FETCH NEXT FROM children_cursor INTO @child_Id  
                          
            SET @Fetch =@@FETCH_STATUS  
                          
                WHILE @Fetch = 0  
                          
            BEGIN  
                print @child_id  
                print @language_branch_id  
                exec[dbo].[lunchin_ChangePageBranchMasterLanguage] @child_id, @language_branch_id, @recursive, @switch_only  
                FETCH NEXT FROM children_cursor INTO @child_Id  
                SET @Fetch =@@FETCH_STATUS  
            END  
                          
            CLOSE children_cursor  
                          
            DEALLOCATE children_cursor  
        END
END
GO

CREATE OR ALTER PROCEDURE[dbo].[lunchin_GetContentBlocks] 
    @page_id int
AS
BEGIN 
                          
    SET NOCOUNT ON; 
    SELECT tblContent.pkID, tblContent.ContentType 
    FROM tblContent
    INNER JOIN tblContentSoftlink ON tblContent.ContentGUID = tblContentSoftlink.fkReferencedContentGUID
    WHERE tblContentSoftlink.fkOwnerContentID = @page_id AND tblContent.ContentType = 1
    ORDER BY tblContent.pkID
END
GO

CREATE OR ALTER PROCEDURE [dbo].[lunchin_GetContentHierarchy]  
    @page_id    int  
AS  
BEGIN  

    SET NOCOUNT ON;  
                            
    WITH content  
    AS
    (
        SELECT Parent.pkID, Parent.ContentGUID, Parent.ContentType, Parent.fkParentID  
        FROM tblContent As Parent  
        WHERE Parent.pkID = @page_id  
                            
        UNION ALL
            
        SELECT Child.pkID, Child.ContentGUID, Child.ContentType, Child.fkParentID  
        FROM tblContent as Child  
        INNER JOIN content  ON Child.fkParentID = content.pkID  
        WHERE Child.fkParentID IS NOT NULL
    )
        
    SELECT *  
    FROM content  
    ORDER BY content.pkID  
                            
END  
GO
