using MechanicaCore.core.ecs;
using MechanicaCore.core.utils;
using Terraria.ModLoader;

namespace MechanicaCore;

public class MechanicaCore : Mod
{
}

public class MechanicaDebugger : ModSystem
{
  public override void Load()
  {
    var runner = new TestRunner(16);

    #region Identity Tests
    runner.AddTest("Identity Creation Test", () =>
    {
      var id = new Identity(1, 2, 3);
      TestRunner.AssertEqual(1u, id.Index, "Index should be 1.");
      TestRunner.AssertEqual((ushort)2, id.Generation, "Generation should be 2.");
      TestRunner.AssertEqual((short)3, id.TypeID, "TypeId should be 3.");
    });

    runner.AddTest("Identity Successor Test", () =>
    {
      var id = new Identity(1, 2, 3);
      var successor = id.Successor();
      TestRunner.AssertEqual((ushort)3, successor.Generation, "Generation should increment by 1.");
      TestRunner.AssertEqual(1u, successor.Index, "Index should remain the same.");
      TestRunner.AssertEqual((short)3, successor.TypeID, "TypeId should remain the same.");
    });

    runner.AddTest("Equality Test", () =>
    {
      var id1 = new Identity(1, 2, 3);
      var id2 = new Identity(1, 2, 3);
      TestRunner.AssertTrue(id1 == id2, "Identities should be equal.");
      TestRunner.AssertFalse(id1 != id2, "Identities should not be unequal.");
    });

    runner.AddTest("Serialization Test", () =>
    {
      var id = new Identity(1, 2, 3);
      var buffer = new byte[8];
      id.Serialize(buffer);
      var deserialized = Identity.Deserialize(buffer);
      TestRunner.AssertEqual(id, deserialized, "Deserialized identity should match original.");
    });
    #endregion

    runner.RunAll();
  }
}