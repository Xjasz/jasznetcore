using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static JaszCore.Models.DatabaseAttribute;
using static JaszCore.Models.OrgTableAttribute;

namespace JaszCore.Models.Main
{
    [OrgTable("xjzKeyMapper", CONNECTION_TYPE.JASZ_MAIN)]
    public class XjzKeyMapper : BaseDataModel<XjzKeyMapper>
    {
        [Key]
        [Column("cfg_id")]
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "P Name")]
        [StringLength(55)]
        [Column("p_name")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string PName { get; set; }

        [Display(Name = "Pub Key")]
        [StringLength(255)]
        [Column("pub_key")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string PubKey { get; set; }

        [Display(Name = "Priv Key")]
        [StringLength(255)]
        [Column("priv_key")]
        [Database(DATABASE_TYPE.COMPARE)]
        public string PrivKey { get; set; }

        public XjzKeyMapper(string p_name, string pub_key, string priv_key)
        {
            PName = p_name;
            PubKey = pub_key;
            PrivKey = priv_key;
        }

        public XjzKeyMapper() { }

        public override int GetID() => ID;
    }
}
