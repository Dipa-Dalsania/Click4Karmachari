
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/10/2019 00:55:43
-- Generated from EDMX file: C:\Users\MBHTech13\Documents\Received Files\ShivaCaterrs\Ecom_Api\dbcontext.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [account5_invdb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Categories_StoreMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Categories] DROP CONSTRAINT [FK_Categories_StoreMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Master_Token_UserMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Master_Token] DROP CONSTRAINT [FK_Master_Token_UserMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductMaster_SubCategories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductMaster] DROP CONSTRAINT [FK_ProductMaster_SubCategories];
GO
IF OBJECT_ID(N'[dbo].[FK_ProductMaster_UOM_Master]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProductMaster] DROP CONSTRAINT [FK_ProductMaster_UOM_Master];
GO
IF OBJECT_ID(N'[dbo].[FK_Purchase_Detail_ProductMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Purchase_Detail] DROP CONSTRAINT [FK_Purchase_Detail_ProductMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Purchase_Detail_Purchase_Master]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Purchase_Detail] DROP CONSTRAINT [FK_Purchase_Detail_Purchase_Master];
GO
IF OBJECT_ID(N'[dbo].[FK_Purchase_Master_StoreMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Purchase_Master] DROP CONSTRAINT [FK_Purchase_Master_StoreMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Purchase_Master_UserMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Purchase_Master] DROP CONSTRAINT [FK_Purchase_Master_UserMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Sales_Detail_ProductMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sales_Detail] DROP CONSTRAINT [FK_Sales_Detail_ProductMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Sales_Detail_Sales_Master1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sales_Detail] DROP CONSTRAINT [FK_Sales_Detail_Sales_Master1];
GO
IF OBJECT_ID(N'[dbo].[FK_Sales_Master_StoreMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sales_Master] DROP CONSTRAINT [FK_Sales_Master_StoreMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_Sales_Master_UserMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sales_Master] DROP CONSTRAINT [FK_Sales_Master_UserMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_SubCategories_Categories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SubCategories] DROP CONSTRAINT [FK_SubCategories_Categories];
GO
IF OBJECT_ID(N'[dbo].[FK_UserMaster_StoreMaster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserMaster] DROP CONSTRAINT [FK_UserMaster_StoreMaster];
GO
IF OBJECT_ID(N'[dbo].[FK_UserMaster_UserCategory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserMaster] DROP CONSTRAINT [FK_UserMaster_UserCategory];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Categories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Categories];
GO
IF OBJECT_ID(N'[dbo].[Master_Log]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Master_Log];
GO
IF OBJECT_ID(N'[dbo].[Master_Token]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Master_Token];
GO
IF OBJECT_ID(N'[dbo].[ProductMaster]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProductMaster];
GO
IF OBJECT_ID(N'[dbo].[Purchase_Detail]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Purchase_Detail];
GO
IF OBJECT_ID(N'[dbo].[Purchase_Master]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Purchase_Master];
GO
IF OBJECT_ID(N'[dbo].[Sales_Detail]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sales_Detail];
GO
IF OBJECT_ID(N'[dbo].[Sales_Master]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sales_Master];
GO
IF OBJECT_ID(N'[dbo].[StoreMaster]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StoreMaster];
GO
IF OBJECT_ID(N'[dbo].[SubCategories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SubCategories];
GO
IF OBJECT_ID(N'[dbo].[UOM_Master]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UOM_Master];
GO
IF OBJECT_ID(N'[dbo].[UserCategory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserCategory];
GO
IF OBJECT_ID(N'[dbo].[UserMaster]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserMaster];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [Category_id] bigint IDENTITY(1,1) NOT NULL,
    [Storeid] bigint  NOT NULL,
    [Category_name] varchar(50)  NOT NULL,
    [status] int  NOT NULL
);
GO

-- Creating table 'Master_Log'
CREATE TABLE [dbo].[Master_Log] (
    [id_log] bigint IDENTITY(1,1) NOT NULL,
    [log_type] varchar(500)  NULL,
    [variable] varchar(200)  NULL,
    [old_value] varchar(8000)  NULL,
    [new_value] varchar(8000)  NULL,
    [log_message] varchar(max)  NULL,
    [addedon] datetime  NULL,
    [addedby] varchar(500)  NULL
);
GO

-- Creating table 'Master_Token'
CREATE TABLE [dbo].[Master_Token] (
    [access_id] bigint IDENTITY(1,1) NOT NULL,
    [User_ID] bigint  NULL,
    [Token] varchar(100)  NULL,
    [Public_key] varchar(100)  NULL,
    [Private_key] varchar(100)  NULL,
    [IP_Address] varchar(50)  NULL,
    [CreatedOn] datetime  NULL,
    [Expire] datetime  NULL,
    [Mac_Address] varchar(100)  NULL,
    [Device_Address] varchar(100)  NULL,
    [API_Source] int  NULL
);
GO

-- Creating table 'ProductMasters'
CREATE TABLE [dbo].[ProductMasters] (
    [Product_ID] bigint IDENTITY(1,1) NOT NULL,
    [SubCategoryId] bigint  NOT NULL,
    [Name] varchar(50)  NOT NULL,
    [SKU] varchar(50)  NULL,
    [Qty_Stock] decimal(18,2)  NULL,
    [Min_Stock] decimal(18,2)  NULL,
    [UOM_Id] bigint  NULL,
    [Mrp_Price] decimal(18,2)  NULL,
    [Sell_Price] decimal(18,2)  NULL,
    [GST_Rate] int  NULL,
    [Status] int  NOT NULL
);
GO

-- Creating table 'Purchase_Detail'
CREATE TABLE [dbo].[Purchase_Detail] (
    [POD_Srno] bigint IDENTITY(1,1) NOT NULL,
    [Product_ID] bigint  NOT NULL,
    [PO_Srno] bigint  NOT NULL,
    [Qty] decimal(18,2)  NULL,
    [Rate] decimal(18,2)  NULL,
    [Value] decimal(18,2)  NULL,
    [GST_Per] decimal(18,2)  NULL,
    [GST_Value] decimal(18,2)  NULL,
    [Discount] decimal(18,2)  NULL,
    [PurchasePrice] decimal(18,2)  NULL,
    [Received_Qty] decimal(18,2)  NULL,
    [Discount_type] varchar(10)  NULL,
    [Discount_value] decimal(18,2)  NULL,
    [Net_Value] decimal(18,2)  NULL
);
GO

-- Creating table 'Purchase_Master'
CREATE TABLE [dbo].[Purchase_Master] (
    [PO_Srno] bigint IDENTITY(1,1) NOT NULL,
    [PO_Number] varchar(50)  NULL,
    [PO_Date] datetime  NULL,
    [Storeid] bigint  NULL,
    [Supplier_Id] bigint  NULL,
    [CreatedOn] datetime  NULL,
    [CreatedBy] bigint  NULL,
    [ReleaseOn] datetime  NULL,
    [Releaseby] bigint  NULL,
    [OccupiedStatus] int  NULL,
    [LastUpdatedOn] datetime  NULL,
    [TotalGrand] decimal(18,2)  NULL,
    [TotalGST] decimal(18,2)  NULL,
    [TotalDiscount] decimal(18,2)  NULL,
    [TotalNet] decimal(18,2)  NULL,
    [PaymentMode] varchar(50)  NULL,
    [PaymentTermDays] int  NULL,
    [SupplierRefNo] varchar(50)  NULL,
    [OtherRefNo] varchar(50)  NULL,
    [ShipToName] varchar(50)  NULL,
    [ShipToAddress] varchar(50)  NULL,
    [ShipToCity] varchar(50)  NULL,
    [ShipToState] varchar(50)  NULL,
    [ShippingInstruction] varchar(500)  NULL,
    [DeliveryTerms] varchar(50)  NULL,
    [Status] int  NULL,
    [GSTFlag] int  NULL
);
GO

-- Creating table 'Sales_Detail'
CREATE TABLE [dbo].[Sales_Detail] (
    [IND_Srno] bigint IDENTITY(1,1) NOT NULL,
    [IN_Srno] bigint  NULL,
    [Product_ID] bigint  NULL,
    [Qty] decimal(18,2)  NULL,
    [Rate] decimal(18,2)  NULL,
    [Value] decimal(18,2)  NULL,
    [GST_Per] decimal(18,2)  NULL,
    [GST_Value] decimal(18,2)  NULL,
    [Discount] decimal(18,2)  NULL,
    [Discount_type] varchar(10)  NULL,
    [Discount_value] decimal(18,2)  NULL,
    [Net_Value] decimal(18,2)  NULL,
    [Dispatched_Qty] decimal(18,2)  NULL
);
GO

-- Creating table 'Sales_Master'
CREATE TABLE [dbo].[Sales_Master] (
    [IN_Srno] bigint IDENTITY(1,1) NOT NULL,
    [IN_Number] varchar(50)  NULL,
    [IN_Date] datetime  NULL,
    [Storeid] bigint  NULL,
    [Customer_Id] bigint  NULL,
    [CreatedOn] datetime  NULL,
    [CreatedBy] bigint  NULL,
    [ReleaseOn] datetime  NULL,
    [Releaseby] bigint  NULL,
    [DispatchedStatus] int  NULL,
    [LastUpdatedOn] datetime  NULL,
    [TotalGrand] decimal(18,2)  NULL,
    [TotalGST] decimal(18,2)  NULL,
    [TotalDiscount] decimal(18,2)  NULL,
    [TotalNet] decimal(18,2)  NULL,
    [PaymentMode] varchar(50)  NULL,
    [PaymentTermDays] int  NULL,
    [SupplierRefNo] varchar(50)  NULL,
    [OtherRefNo] varchar(50)  NULL,
    [ShipToName] varchar(50)  NULL,
    [ShipToAddress] varchar(50)  NULL,
    [ShipToCity] varchar(50)  NULL,
    [ShipToState] varchar(50)  NULL,
    [ShippingInstruction] varchar(500)  NULL,
    [DeliveryTerms] varchar(50)  NULL,
    [Status] int  NULL,
    [GSTFlag] int  NULL
);
GO

-- Creating table 'StoreMasters'
CREATE TABLE [dbo].[StoreMasters] (
    [storeid] bigint IDENTITY(1,1) NOT NULL,
    [storename] varchar(50)  NOT NULL,
    [UniqueID] varchar(50)  NULL,
    [storestatus] bigint  NULL,
    [Regdate] nchar(10)  NULL
);
GO

-- Creating table 'SubCategories'
CREATE TABLE [dbo].[SubCategories] (
    [SubCategoryId] bigint IDENTITY(1,1) NOT NULL,
    [Category_id] bigint  NOT NULL,
    [scatname] varchar(50)  NOT NULL,
    [status] int  NOT NULL
);
GO

-- Creating table 'UOM_Master'
CREATE TABLE [dbo].[UOM_Master] (
    [UOM_Id] bigint IDENTITY(1,1) NOT NULL,
    [UOM_Name] varchar(50)  NOT NULL,
    [status] int  NOT NULL
);
GO

-- Creating table 'UserCategories'
CREATE TABLE [dbo].[UserCategories] (
    [Bcategory_id] bigint IDENTITY(1,1) NOT NULL,
    [Bcategory_name] varchar(50)  NOT NULL,
    [img_url] varchar(max)  NULL,
    [img_name] varchar(500)  NULL,
    [addedby] varchar(500)  NULL,
    [addedon] datetime  NULL,
    [status] int  NOT NULL
);
GO

-- Creating table 'UserMasters'
CREATE TABLE [dbo].[UserMasters] (
    [User_ID] bigint IDENTITY(1,1) NOT NULL,
    [Storeid] bigint  NOT NULL,
    [User_UID] varchar(100)  NULL,
    [User_FirstName] varchar(500)  NOT NULL,
    [User_LastName] varchar(500)  NOT NULL,
    [User_FullName] varchar(1000)  NOT NULL,
    [User_uname] varchar(50)  NULL,
    [User_Mobile] bigint  NULL,
    [User_Email] varchar(500)  NULL,
    [User_Password] varchar(100)  NULL,
    [User_Passcode] varchar(100)  NULL,
    [AadharNo] varchar(50)  NULL,
    [PanNo] varchar(50)  NULL,
    [VoterNo] varchar(50)  NULL,
    [PassportNo] varchar(50)  NULL,
    [UserCategoryId] bigint  NULL,
    [GSTIN_Number] varchar(50)  NULL,
    [SHOP_Number] varchar(50)  NULL,
    [status] int  NOT NULL,
    [last_otp] bigint  NULL,
    [last_otp_expire] datetime  NULL,
    [last_token] varchar(50)  NULL,
    [last_token_expire] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Category_id] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([Category_id] ASC);
GO

-- Creating primary key on [id_log] in table 'Master_Log'
ALTER TABLE [dbo].[Master_Log]
ADD CONSTRAINT [PK_Master_Log]
    PRIMARY KEY CLUSTERED ([id_log] ASC);
GO

-- Creating primary key on [access_id] in table 'Master_Token'
ALTER TABLE [dbo].[Master_Token]
ADD CONSTRAINT [PK_Master_Token]
    PRIMARY KEY CLUSTERED ([access_id] ASC);
GO

-- Creating primary key on [Product_ID] in table 'ProductMasters'
ALTER TABLE [dbo].[ProductMasters]
ADD CONSTRAINT [PK_ProductMasters]
    PRIMARY KEY CLUSTERED ([Product_ID] ASC);
GO

-- Creating primary key on [POD_Srno] in table 'Purchase_Detail'
ALTER TABLE [dbo].[Purchase_Detail]
ADD CONSTRAINT [PK_Purchase_Detail]
    PRIMARY KEY CLUSTERED ([POD_Srno] ASC);
GO

-- Creating primary key on [PO_Srno] in table 'Purchase_Master'
ALTER TABLE [dbo].[Purchase_Master]
ADD CONSTRAINT [PK_Purchase_Master]
    PRIMARY KEY CLUSTERED ([PO_Srno] ASC);
GO

-- Creating primary key on [IND_Srno] in table 'Sales_Detail'
ALTER TABLE [dbo].[Sales_Detail]
ADD CONSTRAINT [PK_Sales_Detail]
    PRIMARY KEY CLUSTERED ([IND_Srno] ASC);
GO

-- Creating primary key on [IN_Srno] in table 'Sales_Master'
ALTER TABLE [dbo].[Sales_Master]
ADD CONSTRAINT [PK_Sales_Master]
    PRIMARY KEY CLUSTERED ([IN_Srno] ASC);
GO

-- Creating primary key on [storeid] in table 'StoreMasters'
ALTER TABLE [dbo].[StoreMasters]
ADD CONSTRAINT [PK_StoreMasters]
    PRIMARY KEY CLUSTERED ([storeid] ASC);
GO

-- Creating primary key on [SubCategoryId] in table 'SubCategories'
ALTER TABLE [dbo].[SubCategories]
ADD CONSTRAINT [PK_SubCategories]
    PRIMARY KEY CLUSTERED ([SubCategoryId] ASC);
GO

-- Creating primary key on [UOM_Id] in table 'UOM_Master'
ALTER TABLE [dbo].[UOM_Master]
ADD CONSTRAINT [PK_UOM_Master]
    PRIMARY KEY CLUSTERED ([UOM_Id] ASC);
GO

-- Creating primary key on [Bcategory_id] in table 'UserCategories'
ALTER TABLE [dbo].[UserCategories]
ADD CONSTRAINT [PK_UserCategories]
    PRIMARY KEY CLUSTERED ([Bcategory_id] ASC);
GO

-- Creating primary key on [User_ID] in table 'UserMasters'
ALTER TABLE [dbo].[UserMasters]
ADD CONSTRAINT [PK_UserMasters]
    PRIMARY KEY CLUSTERED ([User_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Storeid] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [FK_Categories_StoreMaster]
    FOREIGN KEY ([Storeid])
    REFERENCES [dbo].[StoreMasters]
        ([storeid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Categories_StoreMaster'
CREATE INDEX [IX_FK_Categories_StoreMaster]
ON [dbo].[Categories]
    ([Storeid]);
GO

-- Creating foreign key on [Category_id] in table 'SubCategories'
ALTER TABLE [dbo].[SubCategories]
ADD CONSTRAINT [FK_SubCategories_Categories]
    FOREIGN KEY ([Category_id])
    REFERENCES [dbo].[Categories]
        ([Category_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SubCategories_Categories'
CREATE INDEX [IX_FK_SubCategories_Categories]
ON [dbo].[SubCategories]
    ([Category_id]);
GO

-- Creating foreign key on [User_ID] in table 'Master_Token'
ALTER TABLE [dbo].[Master_Token]
ADD CONSTRAINT [FK_Master_Token_UserMaster]
    FOREIGN KEY ([User_ID])
    REFERENCES [dbo].[UserMasters]
        ([User_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Master_Token_UserMaster'
CREATE INDEX [IX_FK_Master_Token_UserMaster]
ON [dbo].[Master_Token]
    ([User_ID]);
GO

-- Creating foreign key on [SubCategoryId] in table 'ProductMasters'
ALTER TABLE [dbo].[ProductMasters]
ADD CONSTRAINT [FK_ProductMaster_SubCategories]
    FOREIGN KEY ([SubCategoryId])
    REFERENCES [dbo].[SubCategories]
        ([SubCategoryId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductMaster_SubCategories'
CREATE INDEX [IX_FK_ProductMaster_SubCategories]
ON [dbo].[ProductMasters]
    ([SubCategoryId]);
GO

-- Creating foreign key on [UOM_Id] in table 'ProductMasters'
ALTER TABLE [dbo].[ProductMasters]
ADD CONSTRAINT [FK_ProductMaster_UOM_Master]
    FOREIGN KEY ([UOM_Id])
    REFERENCES [dbo].[UOM_Master]
        ([UOM_Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ProductMaster_UOM_Master'
CREATE INDEX [IX_FK_ProductMaster_UOM_Master]
ON [dbo].[ProductMasters]
    ([UOM_Id]);
GO

-- Creating foreign key on [Product_ID] in table 'Purchase_Detail'
ALTER TABLE [dbo].[Purchase_Detail]
ADD CONSTRAINT [FK_Purchase_Detail_ProductMaster]
    FOREIGN KEY ([Product_ID])
    REFERENCES [dbo].[ProductMasters]
        ([Product_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Purchase_Detail_ProductMaster'
CREATE INDEX [IX_FK_Purchase_Detail_ProductMaster]
ON [dbo].[Purchase_Detail]
    ([Product_ID]);
GO

-- Creating foreign key on [Product_ID] in table 'Sales_Detail'
ALTER TABLE [dbo].[Sales_Detail]
ADD CONSTRAINT [FK_Sales_Detail_ProductMaster]
    FOREIGN KEY ([Product_ID])
    REFERENCES [dbo].[ProductMasters]
        ([Product_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Sales_Detail_ProductMaster'
CREATE INDEX [IX_FK_Sales_Detail_ProductMaster]
ON [dbo].[Sales_Detail]
    ([Product_ID]);
GO

-- Creating foreign key on [PO_Srno] in table 'Purchase_Detail'
ALTER TABLE [dbo].[Purchase_Detail]
ADD CONSTRAINT [FK_Purchase_Detail_Purchase_Master]
    FOREIGN KEY ([PO_Srno])
    REFERENCES [dbo].[Purchase_Master]
        ([PO_Srno])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Purchase_Detail_Purchase_Master'
CREATE INDEX [IX_FK_Purchase_Detail_Purchase_Master]
ON [dbo].[Purchase_Detail]
    ([PO_Srno]);
GO

-- Creating foreign key on [Storeid] in table 'Purchase_Master'
ALTER TABLE [dbo].[Purchase_Master]
ADD CONSTRAINT [FK_Purchase_Master_StoreMaster]
    FOREIGN KEY ([Storeid])
    REFERENCES [dbo].[StoreMasters]
        ([storeid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Purchase_Master_StoreMaster'
CREATE INDEX [IX_FK_Purchase_Master_StoreMaster]
ON [dbo].[Purchase_Master]
    ([Storeid]);
GO

-- Creating foreign key on [Supplier_Id] in table 'Purchase_Master'
ALTER TABLE [dbo].[Purchase_Master]
ADD CONSTRAINT [FK_Purchase_Master_UserMaster]
    FOREIGN KEY ([Supplier_Id])
    REFERENCES [dbo].[UserMasters]
        ([User_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Purchase_Master_UserMaster'
CREATE INDEX [IX_FK_Purchase_Master_UserMaster]
ON [dbo].[Purchase_Master]
    ([Supplier_Id]);
GO

-- Creating foreign key on [IN_Srno] in table 'Sales_Detail'
ALTER TABLE [dbo].[Sales_Detail]
ADD CONSTRAINT [FK_Sales_Detail_Sales_Master1]
    FOREIGN KEY ([IN_Srno])
    REFERENCES [dbo].[Sales_Master]
        ([IN_Srno])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Sales_Detail_Sales_Master1'
CREATE INDEX [IX_FK_Sales_Detail_Sales_Master1]
ON [dbo].[Sales_Detail]
    ([IN_Srno]);
GO

-- Creating foreign key on [Storeid] in table 'Sales_Master'
ALTER TABLE [dbo].[Sales_Master]
ADD CONSTRAINT [FK_Sales_Master_StoreMaster]
    FOREIGN KEY ([Storeid])
    REFERENCES [dbo].[StoreMasters]
        ([storeid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Sales_Master_StoreMaster'
CREATE INDEX [IX_FK_Sales_Master_StoreMaster]
ON [dbo].[Sales_Master]
    ([Storeid]);
GO

-- Creating foreign key on [Customer_Id] in table 'Sales_Master'
ALTER TABLE [dbo].[Sales_Master]
ADD CONSTRAINT [FK_Sales_Master_UserMaster]
    FOREIGN KEY ([Customer_Id])
    REFERENCES [dbo].[UserMasters]
        ([User_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Sales_Master_UserMaster'
CREATE INDEX [IX_FK_Sales_Master_UserMaster]
ON [dbo].[Sales_Master]
    ([Customer_Id]);
GO

-- Creating foreign key on [Storeid] in table 'UserMasters'
ALTER TABLE [dbo].[UserMasters]
ADD CONSTRAINT [FK_UserMaster_StoreMaster]
    FOREIGN KEY ([Storeid])
    REFERENCES [dbo].[StoreMasters]
        ([storeid])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserMaster_StoreMaster'
CREATE INDEX [IX_FK_UserMaster_StoreMaster]
ON [dbo].[UserMasters]
    ([Storeid]);
GO

-- Creating foreign key on [UserCategoryId] in table 'UserMasters'
ALTER TABLE [dbo].[UserMasters]
ADD CONSTRAINT [FK_UserMaster_UserCategory]
    FOREIGN KEY ([UserCategoryId])
    REFERENCES [dbo].[UserCategories]
        ([Bcategory_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserMaster_UserCategory'
CREATE INDEX [IX_FK_UserMaster_UserCategory]
ON [dbo].[UserMasters]
    ([UserCategoryId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------