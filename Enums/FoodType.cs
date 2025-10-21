namespace op_homework_snake_game.Enums;

public enum FoodType
{
    Normal,     // +1 score, +1 length
    Bonus,      // +5 score, +1 length
    Speed,      // +1 score, +1 length, temporary boost
    Slow,       // +1 score, +1 length, temporary slowness
    Reverse,    // +3 score, +1 length, reversed movement
    Shield,     // +2 score, +1 length, immune to walls and barriers
    Double,     // +2 score, +1 length, x2 score and length added for next food
    Shrink,     // +5 score, +1 length, shrinks by 3 (length after >= 3)
    None
}