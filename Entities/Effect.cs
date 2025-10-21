using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Entities;

public class Effect(FoodType effectType, int duration, int playerId)
{
    public FoodType EffectType { get; } = effectType;
    public int Duration { get; set; } = duration;
    public int PlayerId { get; } = playerId;
}

