﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cost.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Cost3Entities : DbContext
    {
        public Cost3Entities()
            : base("name=Cost3Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<AssignFactory> AssignFactory { get; set; }
        public DbSet<Factory> Factory { get; set; }
        public DbSet<Labour> Labour { get; set; }
        public DbSet<RawStock> RawStock { get; set; }
        public DbSet<RawStockQty> RawStockQty { get; set; }
        public DbSet<VersionManagement> VersionManagement { get; set; }
        public DbSet<WorkCenter> WorkCenter { get; set; }
        public DbSet<CostSumByProductVersion> CostSumByProductVersion { get; set; }
        public DbSet<DetailLabour> DetailLabour { get; set; }
        public DbSet<DetailRawStock> DetailRawStock { get; set; }
        public DbSet<UnfinishedLabour> UnfinishedLabour { get; set; }
        public DbSet<UnfinishedRawStock> UnfinishedRawStock { get; set; }
        public DbSet<BOM> BOM { get; set; }
    }
}
