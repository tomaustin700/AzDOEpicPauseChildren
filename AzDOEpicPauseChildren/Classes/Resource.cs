using System;

namespace AzDOEpicPauseChildren.Classes
{
    public class Resource
    {
        public int id { get; set; }
        public int workItemId { get; set; }
        public int rev { get; set; }
        public Revisedby revisedBy { get; set; }
        public DateTime revisedDate { get; set; }
        public Fields fields { get; set; }
        public _Links1 _links { get; set; }
        public string url { get; set; }
        public Revision revision { get; set; }
    }

}
