
void GiveItem(int protoId, int amount = 1) {
    var item = GameSystems.MapObject.CreateObject(protoId, PartyLeader.GetLocation());
    item.SetInt32(obj_f.item_quantity, amount);
    GameSystems.Item.SetItemParent(item, PartyLeader);
}

GiveItem(8010, 5);
GiveItem(4211, 25);
