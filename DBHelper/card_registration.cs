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
    
    public partial class card_registration
    {
        public int id { get; set; }
        public string TeacherId { get; set; }
        public int calssId { get; set; }
        public string OneCardId { get; set; }
        public string Status { get; set; }
        public System.DateTime UpdateTime { get; set; }
    
        public virtual classdetail classdetail { get; set; }
    }
}
