using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using Newtonsoft.Json;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Engine;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Inventory;
using Sitecore.Commerce.Sample;
using Sitecore.Commerce.Sample.Console;
using Sitecore.Commerce.Sample.Contexts;
using Sitecore.Commerce.ServiceProxy;
using Policy = CommerceOps.Sitecore.Commerce.Core.Policy;
using RunMinionPolicy = CommerceOps.Sitecore.Commerce.Core.RunMinionPolicy;

namespace Sitecore.Commerce.Extensions
{
    public static class EngineExtensions
    {
        public static readonly Lazy<ShopperContext> AuthoringContext = new Lazy<ShopperContext>(
            () =>
            {
                var context = new CsrSheila().Context;
                context.Environment = EnvironmentConstants.AdventureWorksAuthoring;
                return context;
            });

        public static readonly Lazy<Container> AuthoringContainer =
            new Lazy<Container>(() => AuthoringContext.Value.ShopsContainer());

        public static readonly Lazy<CommerceOps.Sitecore.Commerce.Engine.Container> DevOpsContainer = new Lazy<CommerceOps.Sitecore.Commerce.Engine.Container>(
            () =>
            {
                var devOp = new DevOpAndre();
                return devOp.Context.OpsContainer();
            });

        public static bool IsEntityId<T>(this string entityId)
        {
            var entityPrefix = $"Entity-{typeof(T).Name}-";
            return entityId.StartsWith(entityPrefix);
        }

        public static string ToSellableItemKey(this string productName, string catalogName, string variantName = "")
        {
            return $"{catalogName},{productName},{variantName}";
        }

        public static string ToEntityName<T>(this string entityId)
        {
            var entityPrefix = $"Entity-{typeof(T).Name}-";
            if (entityId.StartsWith(entityPrefix))
            {
                entityId = entityId.Remove(0, entityPrefix.Length);
            }

            return entityId;
        }

        public static string ToEntityId<T>(this string name)
        {
            return $"Entity-{typeof(T).Name}-{name}";
        }

        public static string ToEntityId<T>(this string name, string parentName)
        {
            return $"Entity-{typeof(T).Name}-{parentName}-{name}";
        }

        public static string ToEntityNameWithParent(this string entityId)
        {
            var entityPrefix = $"Entity-{typeof(Category).Name}-";
            if (entityId.StartsWith(entityPrefix))
            {
                entityId = entityId.Remove(0, entityPrefix.Length);
            }

            var parts = entityId.Split('-');
            return parts[parts.Length - 1];
        }

        public static bool EqualsIgnoreCase(this string value1, string value2)
        {
            return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        public static void WaitUntilCompletion(
            this CommerceCommand command,
            params string[] allowedErrorTerms)
        {
            command.Should().NotBeNull();
            command.ResponseCode.Should().Be("Ok");
            WaitUntilCompletion(command.TaskId, allowedErrorTerms);
        }

        public static void WaitUntilCompletion(this CommerceOps.Sitecore.Commerce.Core.Commands.CommerceCommand command, params string[] allowedErrorTerms)
        {
            command.Should().NotBeNull();
            command.ResponseCode.Should().Be("Ok");
            WaitUntilCompletion(command.TaskId, allowedErrorTerms);
        }

        public static void WaitUntilCompletion(int taskId, params string[] allowedErrorTerms)
        {
            using (new SampleMethodScope())
            {
                Console.WriteLine($"Waiting for task {taskId} to complete...");

                var maximumWaitTime = TimeSpan.FromMinutes(10);
                var sw = new Stopwatch();
                sw.Start();

                CommerceOps.Sitecore.Commerce.Core.Commands.CommerceCommand command;
                do
                {
                    Thread.Sleep(15000);
                    command = Proxy.GetValue(DevOpsContainer.Value.CheckCommandStatus(taskId));
                    command.Should().NotBeNull();
                } while (!command.IsCompleted && sw.Elapsed <= maximumWaitTime);

                sw.Stop();
                command.Messages.Should().NotContainErrors(allowedErrorTerms);
                command.IsCompleted.Should()
                    .BeTrue($"The long-running command did not complete within the {maximumWaitTime} time limit.");
                command.IsFaulted.Should().BeFalse("The long-running command faulted.");
                command.IsCanceled.Should().BeFalse("The long-running command was canceled.");

                Console.WriteLine($"Task {command.TaskId} completed in {sw.Elapsed}");
            }
        }

        public static async Task ExportCatalogs(string filePath, string mode = "full")
        {
            using (new SampleMethodScope())
            {
                await RunExport(filePath, mode, 10, "ExportCatalogs");
            }
        }

        public static async Task ExportInventorySets(string filePath, string mode = "full")
        {
            using (new SampleMethodScope())
            {
                await RunExport(filePath, mode, 10, "ExportInventorySets");
            }
        }

        public static async Task ImportCatalogs(string filePath, string mode, params string[] allowedErrorTerms)
        {
            using (new SampleMethodScope())
            {
                await RunImport(filePath, mode, "ImportCatalogs", allowedErrorTerms);
            }
        }

        public static async Task ImportInventorySets(string filePath, string mode, params string[] allowedErrorTerms)
        {
            using (new SampleMethodScope())
            {
                await RunImport(filePath, mode, "ImportInventorySets", allowedErrorTerms);
            }
        }

        public static void AddCatalog(string catalogName, string displayName)
        {
            DeleteCatalogIfExists(catalogName);

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    string.Empty,
                    "Details",
                    "AddCatalog",
                    string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                new ViewProperty
                {
                    Name = "Name",
                    Value = catalogName
                },
                new ViewProperty
                {
                    Name = "DisplayName",
                    Value = "Console UX Catalog"
                }
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();
        }

        public static void DeleteCatalogIfExists(string catalogName)
        {
            var getCatalogResult =
                Proxy.GetValue(AuthoringContainer.Value.Catalogs.ByKey(catalogName));
            if (getCatalogResult != null)
            {
                DeleteCatalog(catalogName);
            }
            else
            {
                ConsoleExtensions.WriteExpectedError();
            }
        }

        public static void DeleteCatalog(string catalogName)
        {
            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    $"Entity-Catalog-{catalogName}",
                    "Details",
                    "DeleteCatalog",
                    $"Entity-Catalog-{catalogName}"));
            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainMessageCode("error");
            result.Messages.Should().NotContainMessageCode("validationerror");

            RunPurgeCatalogsMinion();

            var getCatalogResult = Proxy.GetValue(AuthoringContainer.Value.Catalogs.ByKey(catalogName));

            // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
            (getCatalogResult == null).Should().BeTrue($"The catalog {catalogName} was not deleted");
            ConsoleExtensions.WriteExpectedError();
        }

        public static void RunPurgeCatalogsMinion()
        {
            RunMinionWithRetry(
                "Sitecore.Commerce.Plugin.Catalog.PurgeCatalogsMinion, Sitecore.Commerce.Plugin.Catalog",
                EnvironmentConstants.AdventureWorksMinions);
            Thread.Sleep(1000);
        }

        public static void AddInventorySet(string inventorySetName)
        {
            using (new SampleMethodScope())
            {
                var view = Proxy.GetValue(
                    AuthoringContainer.Value.GetEntityView(
                        string.Empty,
                        "Details",
                        "AddInventorySet",
                        string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty
                    {
                        Name = "Name",
                        Value = inventorySetName
                    },
                    new ViewProperty
                    {
                        Name = "DisplayName",
                        Value = $"{inventorySetName} Console UX Inventory Set"
                    },
                    new ViewProperty
                    {
                        Name = "Description",
                        Value = $"{inventorySetName} Console UX Inventory Set Description"
                    }
                };

                var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().NotContainErrors();

                AssertInventorySetExists(inventorySetName.ToEntityId<InventorySet>());
            }
        }

        public static void DeleteInventorySet(string inventorySetId)
        {
            using (new SampleMethodScope())
            {
                var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(
                        string.Empty,
                        string.Empty,
                        "RemoveInventorySet",
                        inventorySetId));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().NotContainErrors();

                AssertInventorySetNotExists(inventorySetId);
            }
        }

        public static void AssociateCatalogToInventorySet(string inventorySetName, string catalogName)
        {
            var inventorySetId = inventorySetName.ToEntityId<InventorySet>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    inventorySetId,
                    "InventorySetCatalogs",
                    "AssociateCatalog",
                    string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "CatalogName",
                    Value = catalogName
                }
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemExists(inventorySetId, ChildViewNames.InventorySetCatalogs, catalogName);
        }

        public static void DisassociateCatalogFromInventorySet(string inventorySetName, string catalogName)
        {
            var inventorySetId = inventorySetName.ToEntityId<InventorySet>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    inventorySetId,
                    "",
                    "DisassociateCatalog",
                    catalogName));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemNotExists(inventorySetId, ChildViewNames.InventorySetCatalogs, catalogName);
        }

        public static void AssociateSellableItemToInventorySet(
            string inventorySetName,
            string productId,
            int quantity,
            bool preorderable = false,
            DateTimeOffset? preorderAvailabilityDate = null,
            int preorderLimit = 0,
            bool backorderable = false,
            DateTimeOffset? backorderAvailabilityDate = null,
            int backorderLimit = 0)
        {
            var inventorySetId = inventorySetName.ToEntityId<InventorySet>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    inventorySetId,
                    "",
                    "AssociateSellableItemToInventorySet",
                    string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "SellableItem",
                    Value = productId
                },
                new ViewProperty
                {
                    Name = "Quantity",
                    Value = quantity.ToString(),
                    OriginalType = typeof(int).FullName
                },
                new ViewProperty
                {
                    Name = "Preorderable",
                    Value = preorderable.ToString(),
                    OriginalType = typeof(bool).FullName
                },
                new ViewProperty
                {
                    Name = "PreorderAvailabliltyDate",
                    Value = preorderAvailabilityDate.HasValue ? preorderAvailabilityDate.Value.ToString() : null,
                    OriginalType = typeof(DateTimeOffset).FullName
                },
                new ViewProperty
                {
                    Name = "PreorderLimit",
                    Value = preorderLimit.ToString(),
                    OriginalType = typeof(int).FullName
                },
                new ViewProperty
                {
                    Name = "Backorderable",
                    Value = backorderable.ToString(),
                    OriginalType = typeof(bool).FullName
                },
                new ViewProperty
                {
                    Name = "BackorderAvailabilityDate",
                    Value = backorderAvailabilityDate.HasValue ? backorderAvailabilityDate.Value.ToString() : null,
                    OriginalType = typeof(DateTimeOffset).FullName
                },
                new ViewProperty
                {
                    Name = "BackorderLimit",
                    Value = backorderLimit.ToString(),
                    OriginalType = typeof(int).FullName
                },
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemExists(inventorySetId, ChildViewNames.InventorySetSellableItems, productId);
        }

        public static void DisassociateSellableItemFromInventorySet(string inventorySetName, string productId)
        {
            var inventorySetId = inventorySetName.ToEntityId<InventorySet>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    inventorySetId,
                    "",
                    "DisassociateSellableItemFromInventorySet",
                    productId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemNotExists(inventorySetId, ChildViewNames.InventorySetSellableItems, productId);
        }

        public static void EditInventoryInformation(
            string inventorySetName,
            string productId,
            List<ViewProperty> propertiesToEdit)
        {
            var inventorySetId = inventorySetName.ToEntityId<InventorySet>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    inventorySetId,
                    "EditInventory",
                    "EditSellableItemInventory",
                    productId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            propertiesToEdit.Add(view.Properties.First(p => p.Name.Equals("Version")));
            view.Properties = new ObservableCollection<ViewProperty>(propertiesToEdit);

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();
        }

        public static void TransferInventoryInformation(
            string sourceInventorySetId,
            string sourceProductId,
            string targetInventorySetId,
            string targetProductId,
            int quantity)
        {
            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    sourceInventorySetId,
                    "TransferInventory",
                    "TransferInventory",
                    sourceProductId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "ProductId",
                    Value = targetProductId
                },
                new ViewProperty
                {
                    Name = "TargetInventorySet",
                    Value = targetInventorySetId
                },
                new ViewProperty
                {
                    Name = "Quantity",
                    Value = quantity.ToString(),
                    OriginalType = typeof(int).FullName
                }
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();
        }

        public static void AddSellableItemVariant(string variantName, string productName, string parentName, string catalogName)
        {
            var productId = productName.ToEntityId<SellableItem>();

            AssertSellableItemExists(productName, parentName, "", catalogName);

            var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(productId, "Details", "AddSellableItemVariant", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "VariantId",
                    Value = variantName
                },
                new ViewProperty
                {
                    Name = "Name",
                    Value = variantName
                },
                new ViewProperty
                {
                    Name = "DisplayName",
                    Value = $"Console UX {variantName}"
                },
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);
            AssertChildViewItemExists(productId, ChildViewNames.SellableItemVariants, variantName);
        }

        public static void EnableSellableItemVariant(string variantName, string productName, string parentName, string catalogName)
        {
            var productId = productName.ToEntityId<SellableItem>();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);

            var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(productId, "Details", "EnableSellableItemVariant", variantName));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version"))
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);

            var variant = AssertChildViewItemExists(productId, ChildViewNames.SellableItemVariants, variantName);
            var disabledProperty = variant.Properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase("disabled"));
            disabledProperty.Should().NotBeNull($"Variant '{variantName}' property 'disabled' was not found.");
            disabledProperty.Value.Should()
                .Be("false", $"Variant '{variantName}' was not disabled in entity {productId}.");
        }

        public static void DisableSellableItemVariant(string variantName, string productName, string parentName, string catalogName)
        {
            var productId = productName.ToEntityId<SellableItem>();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);

            var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(productId, "Details", "DisableSellableItemVariant", variantName));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version"))
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);

            var variant = AssertChildViewItemExists(productId, ChildViewNames.SellableItemVariants, variantName);
            var disabledProperty = variant.Properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase("disabled"));
            disabledProperty.Should().NotBeNull($"Variant '{variantName}' property 'disabled' was not found.");
            disabledProperty.Value.Should()
                .Be("true", $"Variant '{variantName}' was not disabled in entity {productId}.");
        }

        public static void DeleteSellableItemVariant(string variantName, string productName, string parentName, string catalogName)
        {
            var productId = productName.ToEntityId<SellableItem>();

            AssertSellableItemExists(productName, parentName, variantName, catalogName);

            var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(productId, "Details", "DeleteSellableItemVariant", variantName));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version"))
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, "", catalogName);
            AssertChildViewItemNotExists(productId, ChildViewNames.SellableItemVariants, variantName);
            AssertSellableItemNotExists(productName, parentName, variantName, catalogName);

            ConsoleExtensions.WriteExpectedError();
        }

        public static void AddSellableItem(string productId, string parentId, string parentName, string catalogName)
        {
            var productName = productId.ToEntityName<SellableItem>();

            DeleteSellableItemIfExists(productId, parentId, parentName, catalogName);

            var view = Proxy.GetValue(AuthoringContainer.Value.GetEntityView(parentId, "Details", "AddSellableItem", string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "ProductId",
                    Value = productName
                },
                new ViewProperty
                {
                    Name = "Name",
                    Value = productName
                },
                new ViewProperty
                {
                    Name = "DisplayName",
                    Value = $"Console UX {productName}"
                },
                new ViewProperty
                {
                    Name = "Description",
                    Value = $"Console UX {productName} Description"
                },
                new ViewProperty
                {
                    Name = "Brand",
                    Value = "Console UX Brand"
                },
                new ViewProperty
                {
                    Name = "Manufacturer",
                    Value = "Console UX Manufacturer"
                },
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, "", catalogName);
            AssertChildViewItemExists(parentId, ChildViewNames.SellableItems, productId);
        }

        public static void DeleteSellableItemIfExists(string productId, string parentId, string parentName, string catalogName)
        {
            var key = productId.ToSellableItemKey(catalogName);
            var getResult = Proxy.GetValue(AuthoringContainer.Value.SellableItems.ByKey(key));
            if (getResult != null)
            {
                DeleteSellableItem(productId, parentId, parentName, catalogName);
            }
            else
            {
                ConsoleExtensions.WriteExpectedError();
            }
        }

        public static void DeleteSellableItem(string productId, string parentId, string parentName, string catalogName)
        {
            using (new SampleMethodScope())
            {
                var key = productId.ToSellableItemKey(catalogName);
                var getResult = Proxy.GetValue(AuthoringContainer.Value.SellableItems.ByKey(key));
                getResult.Should().NotBeNull();

                var view = Proxy.GetValue(
                    AuthoringContainer.Value.GetEntityView(
                        parentId,
                        "Details",
                        "DeleteSellableItem",
                        getResult.Id));
                view.Properties = new ObservableCollection<ViewProperty>
                {
                    view.Properties.First(p => p.Name.Equals("Version")),
                    new ViewProperty
                    {
                        Name = "DeleteOption",
                        Value = ""
                    },
                };

                var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().NotContainErrors();

                getResult = Proxy.GetValue(
                    AuthoringContainer.Value.SellableItems.ByKey(
                        $"{parentName},{productId.ToEntityName<SellableItem>()},"));

                // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
                (getResult == null).Should().BeTrue($"The sellable item {productId} was not deleted.");
                ConsoleExtensions.WriteExpectedError();
            }
        }

        public static void AssociateSellableItem(string productId, string parentId, string parentName, string catalogName)
        {
            var productName = productId.ToEntityName<SellableItem>();

            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    parentId,
                    "Details",
                    "AssociateSellableItemToCatalog",
                    string.Empty));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "SellableItem",
                    Value = productId
                }
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertSellableItemExists(productName, parentName, "", catalogName);
            AssertChildViewItemExists(parentId, ChildViewNames.SellableItems, productId);
        }

        public static void DisassociateItem(string subjectId, string subjectName, string parentId, string parentName)
        {
            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    parentId,
                    "Details",
                    "DisassociateItem",
                    subjectId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version")),
                new ViewProperty
                {
                    Name = "SellableItem",
                    Value = subjectId
                },
                new ViewProperty
                {
                    Name = "Reparent",
                    Value = "false",
                    OriginalType = typeof(bool).FullName
                },
                new ViewProperty
                {
                    Name = "NewParentID",
                    Value = ""
                },
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemNotExists(
                parentId,
                subjectId.IsEntityId<SellableItem>() ? ChildViewNames.SellableItems : ChildViewNames.Categories,
                subjectId);
        }

        public static void AddSellableItemImage(string productId, string parentName, string imageId)
        {
            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    productId,
                    "AddSellableItemImage",
                    "AddSellableItemImage",
                    imageId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version"))
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            // Image ID s prefixed by a | character in the master view.
            AssertChildViewItemExists(productId, ChildViewNames.Images, $"|{imageId}");
        }

        public static void RemoveSellableItemImage(string productId, string parentName, string imageId)
        {
            var view = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    productId,
                    "Details",
                    "RemoveSellableItemImage",
                    imageId));
            view.Should().NotBeNull();
            view.Policies.Should().BeEmpty();
            view.Properties.Should().NotBeEmpty();
            view.ChildViews.Should().BeEmpty();

            view.Properties = new ObservableCollection<ViewProperty>
            {
                view.Properties.First(p => p.Name.Equals("Version"))
            };

            var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
            result.Messages.Should().NotContainErrors();

            AssertChildViewItemNotExists(productId, ChildViewNames.Images, imageId);
        }

        public static void AddCategory(
            string categoryId,
            string parentId,
            string parentName,
            bool deleteIfExists = true)
        {
            using (new SampleMethodScope())
            {
                var categoryName = categoryId.ToEntityNameWithParent();

                if (deleteIfExists)
                {
                    DeleteCategoryIfExists(categoryId);
                }

                var view = Proxy.GetValue(
                    AuthoringContainer.Value.GetEntityView(
                        parentId,
                        "Details",
                        "AddCategory",
                        string.Empty));
                view.Should().NotBeNull();
                view.Policies.Should().BeEmpty();
                view.Properties.Should().NotBeEmpty();
                view.ChildViews.Should().BeEmpty();

                view.Properties = new ObservableCollection<ViewProperty>
                {
                    view.Properties.First(p => p.Name.Equals("Version")),
                    new ViewProperty
                    {
                        Name = "Name",
                        Value = categoryName
                    },
                    new ViewProperty
                    {
                        Name = "DisplayName",
                        Value = $"Console UX Category {categoryName}"
                    },
                    new ViewProperty
                    {
                        Name = "Description",
                        Value = $"Console UX Category {categoryName} Description"
                    }
                };

                var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().NotContainErrors();

                AssertCategoryExists(categoryId);
                AssertChildViewItemExists(parentId, ChildViewNames.Categories, categoryId);
            }
        }

        public static void DeleteCategoryIfExists(string categoryId)
        {
            var getCategoryResult =
                Proxy.GetValue(AuthoringContainer.Value.Categories.ByKey(categoryId));
            if (getCategoryResult != null)
            {
                DeleteCategory(categoryId);
            }
            else
            {
                ConsoleExtensions.WriteExpectedError();
            }
        }

        public static void DeleteCategory(string categoryId)
        {
            using (new SampleMethodScope())
            {
                var getCategoryResult =
                    Proxy.GetValue(AuthoringContainer.Value.Categories.ByKey(categoryId));
                getCategoryResult.Should().NotBeNull();

                var selectDeleteOptionView = Proxy.GetValue(
                    AuthoringContainer.Value.GetEntityView(
                        categoryId,
                        "Details",
                        "SelectCategoryDeleteOption",
                        categoryId));
                selectDeleteOptionView.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty
                    {
                        Name = "Version",
                        Value = getCategoryResult.Version.ToString(),
                        OriginalType = "System.Int32"
                    },
                };

                var selectDeleteOptionResult = Proxy.DoCommand(
                    AuthoringContainer.Value.DoAction(selectDeleteOptionView));
                selectDeleteOptionResult.Messages.Should().NotContainErrors();

                var view = Proxy.GetValue(
                    AuthoringContainer.Value.GetEntityView(
                        getCategoryResult.Id,
                        "Details",
                        "DeleteCategory",
                        getCategoryResult.Id));
                view.Properties = new ObservableCollection<ViewProperty>
                {
                    new ViewProperty
                    {
                        Name = "Version",
                        Value = (getCategoryResult.Version + 1).ToString(),
                        OriginalType = "System.Int32"
                    },
                };

                var result = Proxy.DoCommand(AuthoringContainer.Value.DoAction(view));
                result.Messages.Should().NotContainErrors();

                RunPurgeCategoriesMinion();

                getCategoryResult =
                    Proxy.GetValue(AuthoringContainer.Value.Categories.ByKey(categoryId));

                // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
                (getCategoryResult == null).Should().BeTrue($"The category {categoryId} was not deleted");
                ConsoleExtensions.WriteExpectedError();
            }
        }

        public static void RunPurgeCategoriesMinion()
        {
            RunMinionWithRetry(
                "Sitecore.Commerce.Plugin.Catalog.PurgeCategoriesMinion, Sitecore.Commerce.Plugin.Catalog",
                EnvironmentConstants.AdventureWorksMinions);
        }

        public static HttpClient CreateHttpClientAuthoring()
        {
            var hc = new HttpClient();
            hc.DefaultRequestHeaders.Accept.Clear();
            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Program.SitecoreTokenRaw);
            hc.DefaultRequestHeaders.Add("ShopName", AuthoringContext.Value.Shop);
            hc.DefaultRequestHeaders.Add("ShopperId", AuthoringContext.Value.ShopperId);
            hc.DefaultRequestHeaders.Add("CustomerId", AuthoringContext.Value.CustomerId);
            hc.DefaultRequestHeaders.Add("Language", AuthoringContext.Value.Language);
            hc.DefaultRequestHeaders.Add("Currency", AuthoringContext.Value.Currency);
            hc.DefaultRequestHeaders.Add("Environment", AuthoringContext.Value.Environment);
            hc.DefaultRequestHeaders.Add("PolicyKeys", AuthoringContext.Value.PolicyKeys);
            hc.DefaultRequestHeaders.Add("EffectiveDate", AuthoringContext.Value.EffectiveDate.ToString());
            hc.DefaultRequestHeaders.Add("IsRegistered", AuthoringContext.Value.IsRegistered.ToString());
            return hc;
        }

        public static void RunMinionWithRetry(
            string minionType,
            string environmentName,
            bool runChildren = false,
            string withListToWatch = "",
            int attempts = 5)
        {
            using (new SampleMethodScope())
            {
                for (var i = 0; i < attempts; i++)
                {
                    ConsoleExtensions.WriteColoredLine(ConsoleColor.White, $"Running => {minionType}, Attempt #{i + 1}");

                    var policies = new Collection<Policy>
                    {
                        new RunMinionPolicy
                        {
                            RunChildren = runChildren,
                            WithListToWatch = withListToWatch
                        }
                    };
                    var result = Proxy.DoOpsCommand(new MinionRunner().Context.MinionsContainer().RunMinion(minionType, environmentName, policies));
                    result.Should().NotBeNull();
                    result.WaitUntilCompletion();

                    if (!result.Messages.Any(m => m.CommerceTermKey.EqualsIgnoreCase("MinionProcessingSameEntities")))
                    {
                        break;
                    }

                    Thread.Sleep(2500);
                }
            }
        }

        public static void NotContainErrors(
            this GenericCollectionAssertions<CommandMessage> assertion,
            params string[] allowedErrorTerms)
        {
            assertion.NotContainMessageCode("error", allowedErrorTerms);
            assertion.NotContainMessageCode("validationerror", allowedErrorTerms);
        }

        public static void NotContainMessageCode(
            this GenericCollectionAssertions<CommandMessage> assertion,
            string messageCode,
            params string[] allowedErrorTerms)
        {
            var violatingMessages = assertion.Subject.Where(m => m.Code.EqualsIgnoreCase(messageCode));
            if (violatingMessages.All(m => allowedErrorTerms.Contains(m.CommerceTermKey)))
            {
                return;
            }

            violatingMessages.Should()
                .BeEmpty(
                    $"A message with code '{messageCode}' was found in the response messages:  {violatingMessages?.FirstOrDefault()?.Text}");
        }

        public static void ContainMessageCode(
            this GenericCollectionAssertions<CommandMessage> assertion,
            string messageCode)
        {
            var violatingMessage = assertion.Subject.FirstOrDefault(m => m.Code.EqualsIgnoreCase(messageCode));
            violatingMessage.Should().NotBeNull($"A message with code '{messageCode}' was not found in the response");
        }

        public static void NotContainErrors(
            this GenericCollectionAssertions<CommerceOps.Sitecore.Commerce.Core.CommandMessage> assertion,
            params string[] allowedErrorTerms)
        {
            assertion.NotContainMessageCode("error", allowedErrorTerms);
            assertion.NotContainMessageCode("validationerror", allowedErrorTerms);
        }

        public static void NotContainMessageCode(
            this GenericCollectionAssertions<CommerceOps.Sitecore.Commerce.Core.CommandMessage> assertion,
            string messageCode,
            params string[] allowedErrorTerms)
        {
            var violatingMessages = assertion.Subject.Where(m => m.Code.EqualsIgnoreCase(messageCode));
            if (violatingMessages.All(m => allowedErrorTerms.Contains(m.CommerceTermKey)))
            {
                return;
            }

            violatingMessages.Should()
                .BeEmpty(
                    $"A message with code '{messageCode}' was found in the response messages:  {violatingMessages?.FirstOrDefault()?.Text}");
        }

        public static void ContainMessageCode(
            this GenericCollectionAssertions<CommerceOps.Sitecore.Commerce.Core.CommandMessage> assertion,
            string messageCode)
        {
            var violatingMessage = assertion.Subject.FirstOrDefault(m => m.Code.EqualsIgnoreCase(messageCode));
            violatingMessage.Should().NotBeNull($"A message with code '{messageCode}' was not found in the response");
        }

        public static void AssertCatalogExists(string catalogId)
        {
            var getResult = Proxy.GetValue(AuthoringContainer.Value.Catalogs.ByKey(catalogId));
            getResult.Should().NotBeNull($"The Catalog {catalogId} does not exist");
        }

        public static void AssertCatalogNotExists(string catalogId)
        {
            var getResult = Proxy.GetValue(AuthoringContainer.Value.Catalogs.ByKey(catalogId));

            // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
            (getResult == null).Should().BeTrue($"The Catalog {catalogId} should not exist");
        }

        public static InventorySet AssertInventorySetExists(string inventorySetId)
        {
            var getResult = Proxy.GetValue(
                AuthoringContainer.Value.InventorySets.ByKey(inventorySetId));
            getResult.Should().NotBeNull($"The InventorySet {inventorySetId} does not exist");
            return getResult;
        }

        public static void AssertInventorySetNotExists(string inventorySetId)
        {
            var getResult = Proxy.GetValue(
                AuthoringContainer.Value.InventorySets.ByKey(inventorySetId));

            // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
            (getResult == null).Should().BeTrue($"The InventorySet {inventorySetId} should not exist");
        }

        public static void AssertCategoryExists(string categoryId)
        {
            var getResult = Proxy.GetValue(AuthoringContainer.Value.Categories.ByKey(categoryId));
            getResult.Should().NotBeNull($"The Category {categoryId} does not exist");
        }

        public static void AssertCategoryNotExists(string categoryId)
        {
            var getResult = Proxy.GetValue(AuthoringContainer.Value.Categories.ByKey(categoryId));

            // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
            (getResult == null).Should().BeTrue($"The Category {categoryId} should not exist");
        }

        public static void AssertSellableItemExists(string productName, string parentName, string variantName, string catalogName)
        {
            var key = productName.ToSellableItemKey(catalogName, variantName);
            var getResult = Proxy.GetValue(AuthoringContainer.Value.SellableItems.ByKey(key));
            getResult.Should().NotBeNull($"The SellableItem with key '{key}' does not exist");
        }

        public static void AssertSellableItemNotExists(string productName, string parentName, string variantName, string catalogName)
        {
            var key = productName.ToSellableItemKey(catalogName, variantName);
            var getResult = Proxy.GetValue(AuthoringContainer.Value.SellableItems.ByKey(key));

            // fix for odata - Fluent assertions causes an Odata exception when a null check fails.
            (getResult == null).Should().BeTrue($"The SellableItem with key '{key}' should not exist");
        }

        public static EntityView AssertChildViewItemExists(string entityId, string childViewName, string childItemId)
        {
            var parentView = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    entityId,
                    "Master",
                    string.Empty,
                    string.Empty));
            var childView = (EntityView) parentView.ChildViews.First(m => m.Name.EqualsIgnoreCase(childViewName));
            var childItem = (EntityView) childView.ChildViews.FirstOrDefault(
                m => (m as EntityView)?.ItemId.EqualsIgnoreCase(childItemId) ?? false);
            childItem.Should()
                .NotBeNull(
                    $"Entity '{entityId}' does not contain an item with ItemId '{childItemId}' in the child view '{childViewName}'.");
            return childItem;
        }

        public static void AssertChildViewItemNotExists(string entityId, string childViewName, string childItemId)
        {
            var parentView = Proxy.GetValue(
                AuthoringContainer.Value.GetEntityView(
                    entityId,
                    "Master",
                    string.Empty,
                    string.Empty));
            var childView = (EntityView) parentView.ChildViews.First(m => m.Name.EqualsIgnoreCase(childViewName));
            var childItem = childView.ChildViews.FirstOrDefault(
                m => (m as EntityView)?.ItemId.EqualsIgnoreCase(childItemId) ?? false);
            childItem.Should()
                .BeNull(
                    $"Entity '{entityId}' should not contain an item with ItemId '{childItemId}' in the child view '{childViewName}'.");
        }

        private static async Task RunExport(string filePath, string mode, int itemsPerFile, string exportMethodName)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Exists(filePath).Should().BeFalse();

            using (var hc = CreateHttpClientAuthoring())
            {
                var exportParameters = new Dictionary<string, string>
                {
                    {
                        "fileName", filePath
                    },
                    {
                        "mode", mode
                    },
                    {
                        "maximumItemsPerFile", itemsPerFile.ToString()
                    }
                };

                using (var exportContent = new StringContent(
                    JsonConvert.SerializeObject(exportParameters),
                    Encoding.UTF8,
                    "application/json"))
                {
                    var exportResponse = await hc.PostAsync(
                        $"{Program.ShopsServiceUri}{exportMethodName}()",
                        exportContent);
                    exportResponse.IsSuccessStatusCode.Should().BeTrue();
                    using (var exportStream = await exportResponse.Content.ReadAsStreamAsync())
                    {
                        using (var exportFile = File.Create(filePath))
                        {
                            exportStream.CopyTo(exportFile);
                        }
                    }
                }
            }

            File.Exists(filePath).Should().BeTrue();
        }

        private static async Task RunImport(
            string filePath,
            string mode,
            string importMethodName,
            params string[] allowedErrorTerms)
        {
            using (new SampleMethodScope())
            {
                File.Exists(filePath).Should().BeTrue();
                using (var importStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var hc = CreateHttpClientAuthoring())
                    {
                        using (var importContent = new MultipartFormDataContent())
                        {
                            importContent.Add(new StreamContent(importStream), "importFile", filePath);
                            importContent.Add(new StringContent(mode), "mode");
                            importContent.Add(new StringContent("0"), "errorThreshold");
                            importContent.Add(new StringContent("true"), "publish");

                            var importResponse = await hc.PostAsync(
                                $"{Program.ShopsServiceUri}{importMethodName}()",
                                importContent);
                            importResponse.IsSuccessStatusCode.Should().BeTrue();
                            var importResponseText = await importResponse.Content.ReadAsStringAsync();
                            var importCommand = JsonConvert.DeserializeObject<CommerceOps.Sitecore.Commerce.Core.Commands.CommerceCommand>(importResponseText);
                            importCommand.WaitUntilCompletion(allowedErrorTerms);
                        }
                    }
                }
            }
        }
    }
}
