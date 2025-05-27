using JaszCore.Models.Main;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static JaszCore.Models.DatabaseAttribute;
using static JaszCore.Models.OrgTableAttribute;

namespace JaszCore.Models.Outer
{
    [OrgTable("cfgHistoryAlt", CONNECTION_TYPE.JASZ_OUTER)]
    public class ConfigHistory : BaseDataModel<XjzApplication>
    {
        [Key]
        [Column("cfg_history_id")]
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "History Number")]
        [StringLength(12)]
        [Column("hist_no")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string HistNumber { get; set; }


        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("created_date")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }

        public ConfigHistory(string histNumber)
        {
            HistNumber = histNumber;
        }

        public ConfigHistory() { }

        public override int GetID() => ID;
    }
}
