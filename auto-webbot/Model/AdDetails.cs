using System;
using System.Collections.Generic;
using System.Text;

namespace auto_webbot.Model
{
    public class AdDetails
    {
        public AdDetails()
        {
            Categories = new List<string>();
            InputPicturePaths = new List<string>();
            Tags = new List<string>();
            Fulfillments = new List<string>();
            Payments = new List<string>();
            DynamicTextOptions = new List<string>() { "No, I do not want a CARFAX Canada report", "Please Contact" };
        }

        public List<string> Categories { get; set; }
        public List<string> InputPicturePaths { get; set; }
        public string AdTitle { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string AdType { get; set; }
        public List<string> Tags { get; set; }

        // Optional
        public string ForSaleBy { get; set; }
        public string MoreInfo { get; set; }
        public List<string> Fulfillments { get; set; }
        public List<string> Payments { get; set; }
        public string Company { get; set; }
        public string CarYear { get; set; }
        public string CarKm { get; set; }

        // Dynamic
        /// <summary>
        /// This property contains the texts for simple options on the read and post where:
        /// - selenium can find the text in the input page
        /// - selenium can click on the text on the post page to select it
        /// </summary>
        public List<string> DynamicTextOptions { get; set; }

        public string Type { get; set; }
    }
}
