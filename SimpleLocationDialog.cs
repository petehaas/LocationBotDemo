using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Bot.Builder.Location;
using System.Configuration;

namespace LocationBot123
{
    [Serializable]
    public class SimpleLocationDialog : IDialog<object>
    {
        private readonly string simpleChannelId;
        private bool initialConversation;

        public SimpleLocationDialog(string channelId)
        {
            this.simpleChannelId = channelId;
            this.initialConversation = true;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var apiKey = ConfigurationManager.AppSettings["BingLocationAPIKey"];
            var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

            var requiredFields = LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality |
                                 LocationRequiredFields.Region | LocationRequiredFields.Country |
                                 LocationRequiredFields.PostalCode;
            
            await SetHeader(context);

            var prompt = "Where should I ship those cool new shoes?";
            
            var locationDialog = new LocationDialog(apiKey, this.simpleChannelId, prompt, options, requiredFields);

            context.Call(locationDialog, this.ResumeAfterLocationDialogAsync);
        }
        
        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            if (place != null)
            {
                var address = place.GetPostalAddress();
                var formatteAddress = string.Join(", ", new[]
                {
                        address.StreetAddress,
                        address.Locality,
                        address.Region,
                        address.PostalCode,
                        address.Country
                    }.Where(x => !string.IsNullOrEmpty(x)));

                await context.PostAsync("Thanks, I will ship it to " + formatteAddress);
            }

            context.Done<string>(null);
        }

        // show location header
        private async Task SetHeader(IDialogContext context)
        {
      
            context.UserData.SetValue<bool>("InitialConversation", false);

            // Create header card.
            var reply = context.MakeMessage();

            var card = new CardAction();

            reply.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>();
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage()
            {
                Url = "https://static.pexels.com/photos/33148/shoes-lebron-nike-spalding.jpg"
            });

             
            HeroCard plCard = new HeroCard()
            {
                Title = "Lebron James Shoes Size 13",
                Images = cardImages  
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);
            

        }

    }
 
   
}