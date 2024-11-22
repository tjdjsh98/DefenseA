using System.Collections.Generic;

public interface IShop
{
    public int EnableRestockCount { set;get; }
    public int RestockCount { set;get; }
    public List<ShopItem> ShopItemList { set; get; }
    public void RestockShopItems();
    public void Refresh();
    public void SellItem(ShopItem item);

}