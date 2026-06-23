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
                    new InventoryPanel(), Placeholder(PanelId.Alchemy, UiTexts.Alchemy), Placeholder(PanelId.Smithing, UiTexts.Smithing),
                    Placeholder(PanelId.Crafting, UiTexts.Crafting), Placeholder(PanelId.Shop, UiTexts.Shop),
                    Placeholder(PanelId.Auction, UiTexts.Auction), Placeholder(PanelId.Mining, UiTexts.Mining),
                }),
                new PanelCategory(UiTexts.NavCategoryWorld, new IPanel[]
                {
                    Placeholder(PanelId.Map, UiTexts.Map), Placeholder(PanelId.Quest, UiTexts.Quest), Placeholder(PanelId.Npc, UiTexts.Npc),
                    Placeholder(PanelId.Sect, UiTexts.Sect), Placeholder(PanelId.SecretRealm, UiTexts.SecretRealm),
                    Placeholder(PanelId.Bounty, UiTexts.Bounty), Placeholder(PanelId.Companion, UiTexts.Companion),
                }),
                new PanelCategory(UiTexts.NavCategoryAdvanced, new IPanel[]
                {
                    Placeholder(PanelId.Achievement, UiTexts.Achievement), Placeholder(PanelId.Chronicle, UiTexts.Chronicle),
                    Placeholder(PanelId.Ranking, UiTexts.Ranking), Placeholder(PanelId.Pvp, UiTexts.Pvp),
                    Placeholder(PanelId.HeartDemon, UiTexts.HeartDemon),
                }),
                new PanelCategory(UiTexts.NavCategorySystem, new IPanel[] { new SavePanel() }),
            };
        }

        public static IReadOnlyList<IPanel> GetPanels() => GetCategories().SelectMany(c => c.Panels).ToArray();

        private static IPanel Placeholder(PanelId id, string title) => new PlaceholderPanel(id, title);
    }
}
