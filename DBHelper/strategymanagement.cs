//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBHelper
{
    using System;
    using System.Collections.Generic;
    
    public partial class strategymanagement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public strategymanagement()
        {
            this.strategydescriptions = new HashSet<strategydescription>();
        }
    
        public int strategyId { get; set; }
        public string strategyname { get; set; }
        public string StrategyDesc { get; set; }
        public System.DateTime CreationDate { get; set; }
        public sbyte CurrentStatus { get; set; }
        public string strategyType { get; set; }
        public string StrategyLocation { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<strategydescription> strategydescriptions { get; set; }
    }
}
