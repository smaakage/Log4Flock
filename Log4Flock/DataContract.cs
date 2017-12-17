using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Log4Flock
{
    /// <summary>
    /// The payload to send to Stack, which will be serialized to JSON before POSTing.
    /// </summary>
    [DataContract]
    public class Payload
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }
        [DataMember(Name = "attachments")]
        public List<Attachment> Attachments { get; set; }
    }

    /// <summary>
    /// It is possible to create more richly-formatted messages using Attachments.
    /// https://docs.flock.com/display/flockos/Attachment
    /// </summary>
    [DataContract]
    public class Attachment
    {
        /// <summary>
        /// Optional text that should appear within the attachment.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Can either be one of 'good', 'warning', 'danger', or any hex color code.
        /// </summary>
        [DataMember(Name = "color")]
        public string Color { get; set; }

        /// <summary>
        /// Fields are displayed in a table on the message.
        /// </summary>
        [DataMember(Name = "views")]
        public View Views { get; set; }

        [DataMember(Name = "forward")]
        public bool Forward { get; set; }

        [DataMember(Name = "buttons")]
        public List<Button> Buttons { get; set; }

        [DataMember(Name = "downloads")]
        public List<Download> Downloads { get; set; }

        public Attachment(string title)
        {
            Title = title;
        }
        public Attachment()
        { }
    }

    /// <summary>
    /// Fields are displayed in a table on the message.
    /// </summary>
    [DataContract]
    public class View
    {
        [DataMember(Name = "html")]
        public Html Html { get; set; }

        [DataMember(Name = "flockml")]
        public string FlockMl { get; set; }

        public View(Html html = null, string flockMl = null)
        {
            Html = html;
            FlockMl = flockMl;
        }
    }

    [DataContract]
    public class Html
    {
        [DataMember(Name = "inline")]
        public string Inline { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }
    }

    [DataContract]
    public class Button
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "action")]
        public ButtonAction Action { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }
    }

    [DataContract]
    public class ButtonAction
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "desktopType")]
        public string DesktopType { get; set; }

        [DataMember(Name = "mobileType")]
        public string MobileType { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }

    [DataContract]
    public class Download
    {
        [DataMember(Name = "src")]
        public string Src { get; set; }

        [DataMember(Name = "mime")]
        public string Mime { get; set; }

        [DataMember(Name = "filename")]
        public string Filename { get; set; }

        [DataMember(Name = "size")]
        public byte Size { get; set; }
    }
}
