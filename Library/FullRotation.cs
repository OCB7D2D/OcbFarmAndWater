using System;
using UnityEngine;

public static class FullRotation
{

    public enum Side
    {
        up, left, forward,
        down, right, back
    };

    public static Vector3i[] Vector = new Vector3i[6]
    {
        Vector3i.up, Vector3i.left, Vector3i.forward,
        Vector3i.down, Vector3i.right, Vector3i.back
    };

    public static int Mirror(int side)
    {
        if (side < 0) throw new ArgumentOutOfRangeException("side");
        if (side > 5) throw new ArgumentOutOfRangeException("side");
        return side > 2 ? side - 3 : side + 3;
    }

    // Generic rotation function (for full axis rotations only)
    // This does not support any 45 degree rotations (beware!)
    // Function has been unit tested to make sure it is correct!
    public static Vector3i Rotate(int rotation, Vector3i vec)
    {
        switch (rotation)
        {
            case 0: return vec;
            case 1: return new Vector3i(vec.z, vec.y, -vec.x);
            case 2: return new Vector3i(-vec.x, vec.y, -vec.z);
            case 3: return new Vector3i(-vec.z, vec.y, vec.x);
            case 4: return new Vector3i(-vec.x, -vec.y, vec.z);
            case 5: return new Vector3i(-vec.z, -vec.y, -vec.x);
            case 6: return new Vector3i(vec.x, -vec.y, -vec.z);
            case 7: return new Vector3i(vec.z, -vec.y, vec.x);
            case 8: return new Vector3i(-vec.x, vec.z, vec.y);
            case 9: return new Vector3i(-vec.z, -vec.x, vec.y);
            case 10: return new Vector3i(vec.x, -vec.z, vec.y);
            case 11: return new Vector3i(vec.z, vec.x, vec.y);
            case 12: return new Vector3i(-vec.y, vec.x, vec.z);
            case 13: return new Vector3i(-vec.y, vec.z, -vec.x);
            case 14: return new Vector3i(-vec.y, -vec.x, -vec.z);
            case 15: return new Vector3i(-vec.y, -vec.z, vec.x);
            case 16: return new Vector3i(vec.x, vec.z, -vec.y);
            case 17: return new Vector3i(vec.z, -vec.x, -vec.y);
            case 18: return new Vector3i(-vec.x, -vec.z, -vec.y);
            case 19: return new Vector3i(-vec.z, vec.x, -vec.y);
            case 20: return new Vector3i(vec.y, -vec.x, vec.z);
            case 21: return new Vector3i(vec.y, -vec.z, -vec.x);
            case 22: return new Vector3i(vec.y, vec.x, -vec.z);
            case 23: return new Vector3i(vec.y, vec.z, vec.x);
            default: throw new ArgumentOutOfRangeException("rotation");
        }
    }


    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    public static Vector3i GetVector(int side, int rotation)
    {
        switch (side)
        {
            case 0:
                switch (rotation)
                {
                    case 0: return Vector3i.up;
                    case 1: return Vector3i.up;
                    case 2: return Vector3i.up;
                    case 3: return Vector3i.up;
                    case 4: return Vector3i.down;
                    case 5: return Vector3i.down;
                    case 6: return Vector3i.down;
                    case 7: return Vector3i.down;
                    case 8: return Vector3i.forward;
                    case 9: return Vector3i.forward;
                    case 10: return Vector3i.forward;
                    case 11: return Vector3i.forward;
                    case 12: return Vector3i.left;
                    case 13: return Vector3i.left;
                    case 14: return Vector3i.left;
                    case 15: return Vector3i.left;
                    case 16: return Vector3i.back;
                    case 17: return Vector3i.back;
                    case 18: return Vector3i.back;
                    case 19: return Vector3i.back;
                    case 20: return Vector3i.right;
                    case 21: return Vector3i.right;
                    case 22: return Vector3i.right;
                    case 23: return Vector3i.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 1:
                switch (rotation)
                {
                    case 0: return Vector3i.left;
                    case 1: return Vector3i.forward;
                    case 2: return Vector3i.right;
                    case 3: return Vector3i.back;
                    case 4: return Vector3i.right;
                    case 5: return Vector3i.forward;
                    case 6: return Vector3i.left;
                    case 7: return Vector3i.back;
                    case 8: return Vector3i.right;
                    case 9: return Vector3i.up;
                    case 10: return Vector3i.left;
                    case 11: return Vector3i.down;
                    case 12: return Vector3i.down;
                    case 13: return Vector3i.forward;
                    case 14: return Vector3i.up;
                    case 15: return Vector3i.back;
                    case 16: return Vector3i.left;
                    case 17: return Vector3i.up;
                    case 18: return Vector3i.right;
                    case 19: return Vector3i.down;
                    case 20: return Vector3i.up;
                    case 21: return Vector3i.forward;
                    case 22: return Vector3i.down;
                    case 23: return Vector3i.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 2:
                switch (rotation)
                {
                    case 0: return Vector3i.forward;
                    case 1: return Vector3i.right;
                    case 2: return Vector3i.back;
                    case 3: return Vector3i.left;
                    case 4: return Vector3i.forward;
                    case 5: return Vector3i.left;
                    case 6: return Vector3i.back;
                    case 7: return Vector3i.right;
                    case 8: return Vector3i.up;
                    case 9: return Vector3i.left;
                    case 10: return Vector3i.down;
                    case 11: return Vector3i.right;
                    case 12: return Vector3i.forward;
                    case 13: return Vector3i.up;
                    case 14: return Vector3i.back;
                    case 15: return Vector3i.down;
                    case 16: return Vector3i.up;
                    case 17: return Vector3i.right;
                    case 18: return Vector3i.down;
                    case 19: return Vector3i.left;
                    case 20: return Vector3i.forward;
                    case 21: return Vector3i.down;
                    case 22: return Vector3i.back;
                    case 23: return Vector3i.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 3:
                switch (rotation)
                {
                    case 0: return Vector3i.down;
                    case 1: return Vector3i.down;
                    case 2: return Vector3i.down;
                    case 3: return Vector3i.down;
                    case 4: return Vector3i.up;
                    case 5: return Vector3i.up;
                    case 6: return Vector3i.up;
                    case 7: return Vector3i.up;
                    case 8: return Vector3i.back;
                    case 9: return Vector3i.back;
                    case 10: return Vector3i.back;
                    case 11: return Vector3i.back;
                    case 12: return Vector3i.right;
                    case 13: return Vector3i.right;
                    case 14: return Vector3i.right;
                    case 15: return Vector3i.right;
                    case 16: return Vector3i.forward;
                    case 17: return Vector3i.forward;
                    case 18: return Vector3i.forward;
                    case 19: return Vector3i.forward;
                    case 20: return Vector3i.left;
                    case 21: return Vector3i.left;
                    case 22: return Vector3i.left;
                    case 23: return Vector3i.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 4:
                switch (rotation)
                {
                    case 0: return Vector3i.right;
                    case 1: return Vector3i.back;
                    case 2: return Vector3i.left;
                    case 3: return Vector3i.forward;
                    case 4: return Vector3i.left;
                    case 5: return Vector3i.back;
                    case 6: return Vector3i.right;
                    case 7: return Vector3i.forward;
                    case 8: return Vector3i.left;
                    case 9: return Vector3i.down;
                    case 10: return Vector3i.right;
                    case 11: return Vector3i.up;
                    case 12: return Vector3i.up;
                    case 13: return Vector3i.back;
                    case 14: return Vector3i.down;
                    case 15: return Vector3i.forward;
                    case 16: return Vector3i.right;
                    case 17: return Vector3i.down;
                    case 18: return Vector3i.left;
                    case 19: return Vector3i.up;
                    case 20: return Vector3i.down;
                    case 21: return Vector3i.back;
                    case 22: return Vector3i.up;
                    case 23: return Vector3i.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 5:
                switch (rotation)
                {
                    case 0: return Vector3i.back;
                    case 1: return Vector3i.left;
                    case 2: return Vector3i.forward;
                    case 3: return Vector3i.right;
                    case 4: return Vector3i.back;
                    case 5: return Vector3i.right;
                    case 6: return Vector3i.forward;
                    case 7: return Vector3i.left;
                    case 8: return Vector3i.down;
                    case 9: return Vector3i.right;
                    case 10: return Vector3i.up;
                    case 11: return Vector3i.left;
                    case 12: return Vector3i.back;
                    case 13: return Vector3i.down;
                    case 14: return Vector3i.forward;
                    case 15: return Vector3i.up;
                    case 16: return Vector3i.down;
                    case 17: return Vector3i.left;
                    case 18: return Vector3i.up;
                    case 19: return Vector3i.right;
                    case 20: return Vector3i.back;
                    case 21: return Vector3i.up;
                    case 22: return Vector3i.forward;
                    case 23: return Vector3i.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("side");
    }

    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    public static int GetSide(int side, int rotation)
    {
        if (rotation == 0)
            return side;
        switch (side)
        {
            case 0:
                switch (rotation)
                {
                    case 0: return (int)Side.up;
                    case 1: return (int)Side.up;
                    case 2: return (int)Side.up;
                    case 3: return (int)Side.up;
                    case 4: return (int)Side.down;
                    case 5: return (int)Side.down;
                    case 6: return (int)Side.down;
                    case 7: return (int)Side.down;
                    case 8: return (int)Side.forward;
                    case 9: return (int)Side.forward;
                    case 10: return (int)Side.forward;
                    case 11: return (int)Side.forward;
                    case 12: return (int)Side.left;
                    case 13: return (int)Side.left;
                    case 14: return (int)Side.left;
                    case 15: return (int)Side.left;
                    case 16: return (int)Side.back;
                    case 17: return (int)Side.back;
                    case 18: return (int)Side.back;
                    case 19: return (int)Side.back;
                    case 20: return (int)Side.right;
                    case 21: return (int)Side.right;
                    case 22: return (int)Side.right;
                    case 23: return (int)Side.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 1:
                switch (rotation)
                {
                    case 0: return (int)Side.left;
                    case 1: return (int)Side.forward;
                    case 2: return (int)Side.right;
                    case 3: return (int)Side.back;
                    case 4: return (int)Side.right;
                    case 5: return (int)Side.forward;
                    case 6: return (int)Side.left;
                    case 7: return (int)Side.back;
                    case 8: return (int)Side.right;
                    case 9: return (int)Side.up;
                    case 10: return (int)Side.left;
                    case 11: return (int)Side.down;
                    case 12: return (int)Side.down;
                    case 13: return (int)Side.forward;
                    case 14: return (int)Side.up;
                    case 15: return (int)Side.back;
                    case 16: return (int)Side.left;
                    case 17: return (int)Side.up;
                    case 18: return (int)Side.right;
                    case 19: return (int)Side.down;
                    case 20: return (int)Side.up;
                    case 21: return (int)Side.forward;
                    case 22: return (int)Side.down;
                    case 23: return (int)Side.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 2:
                switch (rotation)
                {
                    case 0: return (int)Side.forward;
                    case 1: return (int)Side.right;
                    case 2: return (int)Side.back;
                    case 3: return (int)Side.left;
                    case 4: return (int)Side.forward;
                    case 5: return (int)Side.left;
                    case 6: return (int)Side.back;
                    case 7: return (int)Side.right;
                    case 8: return (int)Side.up;
                    case 9: return (int)Side.left;
                    case 10: return (int)Side.down;
                    case 11: return (int)Side.right;
                    case 12: return (int)Side.forward;
                    case 13: return (int)Side.up;
                    case 14: return (int)Side.back;
                    case 15: return (int)Side.down;
                    case 16: return (int)Side.up;
                    case 17: return (int)Side.right;
                    case 18: return (int)Side.down;
                    case 19: return (int)Side.left;
                    case 20: return (int)Side.forward;
                    case 21: return (int)Side.down;
                    case 22: return (int)Side.back;
                    case 23: return (int)Side.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 3:
                switch (rotation)
                {
                    case 0: return (int)Side.down;
                    case 1: return (int)Side.down;
                    case 2: return (int)Side.down;
                    case 3: return (int)Side.down;
                    case 4: return (int)Side.up;
                    case 5: return (int)Side.up;
                    case 6: return (int)Side.up;
                    case 7: return (int)Side.up;
                    case 8: return (int)Side.back;
                    case 9: return (int)Side.back;
                    case 10: return (int)Side.back;
                    case 11: return (int)Side.back;
                    case 12: return (int)Side.right;
                    case 13: return (int)Side.right;
                    case 14: return (int)Side.right;
                    case 15: return (int)Side.right;
                    case 16: return (int)Side.forward;
                    case 17: return (int)Side.forward;
                    case 18: return (int)Side.forward;
                    case 19: return (int)Side.forward;
                    case 20: return (int)Side.left;
                    case 21: return (int)Side.left;
                    case 22: return (int)Side.left;
                    case 23: return (int)Side.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 4:
                switch (rotation)
                {
                    case 0: return (int)Side.right;
                    case 1: return (int)Side.back;
                    case 2: return (int)Side.left;
                    case 3: return (int)Side.forward;
                    case 4: return (int)Side.left;
                    case 5: return (int)Side.back;
                    case 6: return (int)Side.right;
                    case 7: return (int)Side.forward;
                    case 8: return (int)Side.left;
                    case 9: return (int)Side.down;
                    case 10: return (int)Side.right;
                    case 11: return (int)Side.up;
                    case 12: return (int)Side.up;
                    case 13: return (int)Side.back;
                    case 14: return (int)Side.down;
                    case 15: return (int)Side.forward;
                    case 16: return (int)Side.right;
                    case 17: return (int)Side.down;
                    case 18: return (int)Side.left;
                    case 19: return (int)Side.up;
                    case 20: return (int)Side.down;
                    case 21: return (int)Side.back;
                    case 22: return (int)Side.up;
                    case 23: return (int)Side.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 5:
                switch (rotation)
                {
                    case 0: return (int)Side.back;
                    case 1: return (int)Side.left;
                    case 2: return (int)Side.forward;
                    case 3: return (int)Side.right;
                    case 4: return (int)Side.back;
                    case 5: return (int)Side.right;
                    case 6: return (int)Side.forward;
                    case 7: return (int)Side.left;
                    case 8: return (int)Side.down;
                    case 9: return (int)Side.right;
                    case 10: return (int)Side.up;
                    case 11: return (int)Side.left;
                    case 12: return (int)Side.back;
                    case 13: return (int)Side.down;
                    case 14: return (int)Side.forward;
                    case 15: return (int)Side.up;
                    case 16: return (int)Side.down;
                    case 17: return (int)Side.left;
                    case 18: return (int)Side.up;
                    case 19: return (int)Side.right;
                    case 20: return (int)Side.back;
                    case 21: return (int)Side.up;
                    case 22: return (int)Side.forward;
                    case 23: return (int)Side.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("side");
    }

    // Highly specialized version (can't be much faster?)
    // Function has been unit tested to make sure it is correct!
    public static int InvSide(int side, int rotation)
    {
        if (rotation == 0)
            return side;
        switch (side)
        {
            case 0:
                switch (rotation)
                {
                    case 0: return (int)Side.up;
                    case 1: return (int)Side.up;
                    case 2: return (int)Side.up;
                    case 3: return (int)Side.up;
                    case 4: return (int)Side.down;
                    case 5: return (int)Side.down;
                    case 6: return (int)Side.down;
                    case 7: return (int)Side.down;
                    case 8: return (int)Side.forward;
                    case 9: return (int)Side.left;
                    case 10: return (int)Side.back;
                    case 11: return (int)Side.right;
                    case 12: return (int)Side.right;
                    case 13: return (int)Side.forward;
                    case 14: return (int)Side.left;
                    case 15: return (int)Side.back;
                    case 16: return (int)Side.forward;
                    case 17: return (int)Side.left;
                    case 18: return (int)Side.back;
                    case 19: return (int)Side.right;
                    case 20: return (int)Side.left;
                    case 21: return (int)Side.back;
                    case 22: return (int)Side.right;
                    case 23: return (int)Side.forward;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 1:
                switch (rotation)
                {
                    case 0: return (int)Side.left;
                    case 1: return (int)Side.back;
                    case 2: return (int)Side.right;
                    case 3: return (int)Side.forward;
                    case 4: return (int)Side.right;
                    case 5: return (int)Side.forward;
                    case 6: return (int)Side.left;
                    case 7: return (int)Side.back;
                    case 8: return (int)Side.right;
                    case 9: return (int)Side.forward;
                    case 10: return (int)Side.left;
                    case 11: return (int)Side.back;
                    case 12: return (int)Side.up;
                    case 13: return (int)Side.up;
                    case 14: return (int)Side.up;
                    case 15: return (int)Side.up;
                    case 16: return (int)Side.left;
                    case 17: return (int)Side.back;
                    case 18: return (int)Side.right;
                    case 19: return (int)Side.forward;
                    case 20: return (int)Side.down;
                    case 21: return (int)Side.down;
                    case 22: return (int)Side.down;
                    case 23: return (int)Side.down;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 2:
                switch (rotation)
                {
                    case 0: return (int)Side.forward;
                    case 1: return (int)Side.left;
                    case 2: return (int)Side.back;
                    case 3: return (int)Side.right;
                    case 4: return (int)Side.forward;
                    case 5: return (int)Side.left;
                    case 6: return (int)Side.back;
                    case 7: return (int)Side.right;
                    case 8: return (int)Side.up;
                    case 9: return (int)Side.up;
                    case 10: return (int)Side.up;
                    case 11: return (int)Side.up;
                    case 12: return (int)Side.forward;
                    case 13: return (int)Side.left;
                    case 14: return (int)Side.back;
                    case 15: return (int)Side.right;
                    case 16: return (int)Side.down;
                    case 17: return (int)Side.down;
                    case 18: return (int)Side.down;
                    case 19: return (int)Side.down;
                    case 20: return (int)Side.forward;
                    case 21: return (int)Side.left;
                    case 22: return (int)Side.back;
                    case 23: return (int)Side.right;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 3:
                switch (rotation)
                {
                    case 0: return (int)Side.down;
                    case 1: return (int)Side.down;
                    case 2: return (int)Side.down;
                    case 3: return (int)Side.down;
                    case 4: return (int)Side.up;
                    case 5: return (int)Side.up;
                    case 6: return (int)Side.up;
                    case 7: return (int)Side.up;
                    case 8: return (int)Side.back;
                    case 9: return (int)Side.right;
                    case 10: return (int)Side.forward;
                    case 11: return (int)Side.left;
                    case 12: return (int)Side.left;
                    case 13: return (int)Side.back;
                    case 14: return (int)Side.right;
                    case 15: return (int)Side.forward;
                    case 16: return (int)Side.back;
                    case 17: return (int)Side.right;
                    case 18: return (int)Side.forward;
                    case 19: return (int)Side.left;
                    case 20: return (int)Side.right;
                    case 21: return (int)Side.forward;
                    case 22: return (int)Side.left;
                    case 23: return (int)Side.back;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 4:
                switch (rotation)
                {
                    case 0: return (int)Side.right;
                    case 1: return (int)Side.forward;
                    case 2: return (int)Side.left;
                    case 3: return (int)Side.back;
                    case 4: return (int)Side.left;
                    case 5: return (int)Side.back;
                    case 6: return (int)Side.right;
                    case 7: return (int)Side.forward;
                    case 8: return (int)Side.left;
                    case 9: return (int)Side.back;
                    case 10: return (int)Side.right;
                    case 11: return (int)Side.forward;
                    case 12: return (int)Side.down;
                    case 13: return (int)Side.down;
                    case 14: return (int)Side.down;
                    case 15: return (int)Side.down;
                    case 16: return (int)Side.right;
                    case 17: return (int)Side.forward;
                    case 18: return (int)Side.left;
                    case 19: return (int)Side.back;
                    case 20: return (int)Side.up;
                    case 21: return (int)Side.up;
                    case 22: return (int)Side.up;
                    case 23: return (int)Side.up;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
            case 5:
                switch (rotation)
                {
                    case 0: return (int)Side.back;
                    case 1: return (int)Side.right;
                    case 2: return (int)Side.forward;
                    case 3: return (int)Side.left;
                    case 4: return (int)Side.back;
                    case 5: return (int)Side.right;
                    case 6: return (int)Side.forward;
                    case 7: return (int)Side.left;
                    case 8: return (int)Side.down;
                    case 9: return (int)Side.down;
                    case 10: return (int)Side.down;
                    case 11: return (int)Side.down;
                    case 12: return (int)Side.back;
                    case 13: return (int)Side.right;
                    case 14: return (int)Side.forward;
                    case 15: return (int)Side.left;
                    case 16: return (int)Side.up;
                    case 17: return (int)Side.up;
                    case 18: return (int)Side.up;
                    case 19: return (int)Side.up;
                    case 20: return (int)Side.back;
                    case 21: return (int)Side.right;
                    case 22: return (int)Side.forward;
                    case 23: return (int)Side.left;
                    default: throw new ArgumentOutOfRangeException("rotation");
                }
        }
        throw new ArgumentOutOfRangeException("side");
    }

    // Useful helper function (debugging only)
    public static string VectorToString(Vector3 vector)
    {
        if (vector == Vector3i.up) return "up";
        if (vector == Vector3i.down) return "down";
        if (vector == Vector3i.left) return "left";
        if (vector == Vector3i.right) return "right";
        if (vector == Vector3i.forward) return "forward";
        if (vector == Vector3i.back) return "back";
        return vector.ToString();
    }

    // Useful helper function (debugging only)
    public static int VectorToSide(Vector3 vector)
    {
        if (vector == Vector3i.up) return (int)Side.up;
        if (vector == Vector3i.down) return (int)Side.down;
        if (vector == Vector3i.left) return (int)Side.left;
        if (vector == Vector3i.right) return (int)Side.right;
        if (vector == Vector3i.forward) return (int)Side.forward;
        if (vector == Vector3i.back) return (int)Side.back;
        return -1;
    }

    // Useful helper function (debugging only)
    public static string SideToString(int side)
    {
        if (side == (int)Side.up) return "up";
        if (side == (int)Side.down) return "down";
        if (side == (int)Side.left) return "left";
        if (side == (int)Side.right) return "right";
        if (side == (int)Side.forward) return "forward";
        if (side == (int)Side.back) return "back";
        return side.ToString();
    }

}

