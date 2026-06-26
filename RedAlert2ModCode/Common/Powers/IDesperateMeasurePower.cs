using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Common.Powers;

public interface IDesperateMeasurePower
{
    Task<bool> ExecuteDesperateMeasureAttack(Creature target, PlayerChoiceContext ctx);
}