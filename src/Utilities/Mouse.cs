using Microsoft.Xna.Framework;
using Terraria;

public static class MouseUtils
{
  /// <summary>
  /// Checks if the mouse intersects with the specified rectangle.
  /// </summary>
  /// <param name="other">The rectangle to check for intersection.</param>
  /// <returns>Returns true if the mouse is within the rectangle; otherwise, false.</returns>
  public static bool MouseIntersects(Rectangle other)
  {
    Point mousePosition = new(Main.mouseX, Main.mouseY);

    return other.Contains(mousePosition);
  }
}