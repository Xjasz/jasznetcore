using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static JaszCore.Models.DatabaseAttribute;
using static JaszCore.Models.OrgTableAttribute;

namespace JaszCore.Models.Main
{
    [OrgTable("cfgApplication", CONNECTION_TYPE.JASZ_MAIN)]
    public class ConfigApplication : BaseDataModel<ConfigApplication>
    {
        [Key]
        [Column("cfg_id")]
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "App Name")]
        [StringLength(55)]
        [Column("app_name")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string AppName { get; set; }

        [Display(Name = "Main Value")]
        [StringLength(125)]
        [Column("main_val")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string MainValue { get; set; }

        [Display(Name = "Alt Value")]
        [StringLength(125)]
        [Column("alt_val")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string AltValue { get; set; }

        [Display(Name = "Main Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("main_date")]
        [Database(DATABASE_TYPE.NONE)]
        public DateTime? MainDate { get; set; }

        [Display(Name = "Alt Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("alt_date")]
        [Database(DATABASE_TYPE.NONE)]
        public DateTime? AltDate { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("created_date")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Database(DATABASE_TYPE.NONE)]
        public DateTime CreatedDate { get; set; }

        public ConfigApplication(string appName, string mainValue, DateTime mainDate)
        {
            AppName = appName;
            MainValue = mainValue;
            MainDate = mainDate;
        }

        public ConfigApplication(string appName, string mainValue)
        {
            AppName = appName;
            MainValue = mainValue;
        }

        public ConfigApplication() { }

        public override int GetID() => ID;
    }
}
