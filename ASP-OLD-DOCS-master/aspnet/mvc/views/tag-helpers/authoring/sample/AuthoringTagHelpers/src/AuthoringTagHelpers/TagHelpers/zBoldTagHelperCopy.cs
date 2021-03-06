using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace AuthoringTagHelpers.TagHelpers3
{
   [TargetElement("bold")]
   [TargetElement(Attributes = "bold")]
   public class BoldTagHelper : TagHelper
   {
      public override void Process(TagHelperContext context, TagHelperOutput output)
      {
         output.Attributes.RemoveAll("bold");
         output.PreContent.SetHtmlContent("<strong>");
         output.PostContent.SetHtmlContent("</strong>");
      }
   }
}