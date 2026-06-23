// ============================================================
// PanelRegistry.cs — ordered HUD panel categories for issue #10
// ============================================================

using System.Collections.Generic;
using System.Linq;
using Xiuxian.App;

namespace Xiuxian.UI
{
    public sealed class PanelCategory
    {
        public PanelCategory(string title, IEnumerable<IPanel> panels)
        {
            Title = title;
            Panels = panels.ToArray();
        }

        public string Title { get; }
        public IReadOnlyList<IPanel> Panels { get; }
    }

    public static class PanelRegistry
    {
        public static IReadOnlyList<PanelCategory> GetCategories()
        {
            return new[]
            {
                new PanelCategory(UiTexts.NavCategoryCultivation, new IPanel[]
                {
                    new StatusPanel(), new ActionPanel(), new TalentPanel(), new TechniquePanel(), new DivineArtsPanel(),
                    new EquipmentPanel(), new EnlightenmentPanel(), new LearningPanel(),
                }),
                new PanelCategory(UiTexts.NavCategoryEconomy, new IPanel[]
                {
                    new InventoryPanel(), new AlchemyPanel(), new SmithingPanel(), new CraftingPanel(), new ShopPanel(),
                    new AuctionPanel(), new MiningPanel(),
                }),
                new PanelCategory(UiTexts.NavCategoryWorld, new IPanel[]
                {
                    new MapPanel(), new QuestPanel(), new NpcPanel(),
                    new SectPanel(), new SecretRealmPanel(), new BountyPanel(), new CompanionPanel(),
                }),
                new PanelCategory(UiTexts.NavCategoryAdvanced, new IPanel[]
                {
                    new AchievementPanel(), new ChroniclePanel(), new RankingPanel(), new PvpPanel(), new HeartDemonPanel(),
                }),
                new PanelCategory(UiTexts.NavCategorySystem, new IPanel[] { new SavePanel() }),
            };
        }

        public static IReadOnlyList<IPanel> GetPanels() => GetCategories().SelectMany(c => c.Panels).ToArray();

    }
}
