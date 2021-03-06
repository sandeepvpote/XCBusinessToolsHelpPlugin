using System;
using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Commerce.Sample.Console;

namespace Sitecore.Commerce.Sample.Contexts
{
    public class AnonymousCustomerSteve
    {
        public AnonymousCustomerSteve()
        {
            Context = new ShopperContext
            {
                Shop = Program.DefaultStorefront,
                ShopperId = "ShopperSteveId",
                Environment = EnvironmentConstants.HabitatShops,
                Language = "en-US",
                Currency = "USD",
                PolicyKeys = "ZeroMinionDelay|xActivityPerf",
                EffectiveDate = DateTimeOffset.Now,
                Components = new List<Component>
                {
                    new PhysicalFulfillmentComponent
                    {
                        Id = Guid.NewGuid().ToString(),
                        ShippingParty = new Party
                        {
                            FirstName = "Steve",
                            LastName = "Key",
                            AddressName = "FulfillmentPartyName",
                            Address1 = "326 Duval Street",
                            City = "Key West",
                            StateCode = "WA",
                            State = "Washington",
                            Country = "United States",
                            CountryCode = "US",
                            ZipPostalCode = "33040"
                        },
                        FulfillmentMethod = new EntityReference
                        {
                            EntityTarget = "B146622D-DC86-48A3-B72A-05EE8FFD187A",
                            Name = "Ground"
                        }
                    },
                    new FederatedPaymentComponent
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentMethodNonce = "fake-valid-nonce",
                        BillingParty = new Party
                        {
                            FirstName = "Steve",
                            LastName = "Key",
                            AddressName = "PaymentPartyName",
                            Address1 = "326 Duval Street",
                            City = "Key West",
                            StateCode = "WA",
                            State = "Washington",
                            Country = "United States",
                            CountryCode = "US",
                            ZipPostalCode = "33040"
                        },
                        PaymentMethod = new EntityReference
                        {
                            EntityTarget = "0CFFAB11-2674-4A18-AB04-228B1F8A1DEC",
                            Name = "Federated"
                        }
                    },
                    new ElectronicFulfillmentComponent
                    {
                        Id = Guid.NewGuid().ToString(),
                        EmailAddress = "steve@domain.com",
                        EmailContent = "email message",
                        FulfillmentMethod = new EntityReference
                        {
                            EntityTarget = "8A23234F-8163-4609-BD32-32D9DD6E32F5",
                            Name = "Email"
                        }
                    }
                }
            };
        }

        public ShopperContext Context { get; set; }
    }
}
