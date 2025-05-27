using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static JaszCore.Models.DatabaseAttribute;
using static JaszCore.Models.OrgTableAttribute;

namespace JaszCore.Models.Main
{
    [OrgTable("xjzApplication", CONNECTION_TYPE.JASZ_MAIN)]
    public class XjzApplication : BaseDataModel<XjzApplication>
    {
        [Key]
        [Column("xjz_id")]
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "P Name")]
        [StringLength(55)]
        [Column("p_name")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string PName { get; set; }

        [Display(Name = "Was Used")]
        [Column("was_used")]
        public bool WasUsed { get; set; }

        public XjzApplication(string pName)
        {
            PName = pName;
        }

        public XjzApplication() { }

        public static string GetTableName()
        {
            var tableAttribute = typeof(XjzApplication).GetCustomAttributes(typeof(OrgTableAttribute), true).FirstOrDefault() as OrgTableAttribute;
            return tableAttribute.Name;
        }

        public override int GetID() => ID;
    }
}
