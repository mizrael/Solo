
using System;
using Microsoft.Xna.Framework;

namespace Tetris;

public class PieceGenerator
{
    private int _lastId = 0;
    private Piece _upcoming;
    private Piece _future;

    private Piece Create()
    {
        var template = _templates[Random.Shared.Next(_templates.Length)];
        var color = _colors[Random.Shared.Next(_colors.Length)];

        return new Piece(++_lastId, template, color);
    }

    public Piece Step()
    {
        var tmp = _upcoming;
        _upcoming = _future ?? Create();
        _future = Create();
        return tmp ?? Create();
    }

    public Piece Peek()
        => _upcoming;

    private readonly static Color[] _colors =
    [
        Color.Cyan,
        Color.Blue,
        Color.Orange,
        Color.Yellow,
        Color.Green,
        Color.Purple,
        Color.Red
    ];

    private readonly static PieceTemplate[] _templates;
    static PieceGenerator()
    {
        _templates =
        [
            // Piece I
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { true, true, true, true } }),
                    new Shape(new bool[,] { { true }, { true }, { true }, { true } })
                ]
            ),

            // Piece J
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { true, false, false }, { true, true, true } }),
                    new Shape(new bool[,] { { true, true }, { true, false }, { true, false } }),
                    new Shape(new bool[,] { { true, true, true }, { false, false, true } }),
                    new Shape(new bool[,] { { false, true }, { false, true }, { true, true } })
                ]
            ),

            // Piece L
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { false, false, true }, { true, true, true } }),
                    new Shape(new bool[,] { { true, false }, { true, false }, { true, true } }),
                    new Shape(new bool[,] { { true, true, true }, { true, false, false } }),
                    new Shape(new bool[,] { { true, true }, { false, true }, { false, true } })
                ]
            ),

            // Piece O
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { true, true }, { true, true } })
                ]
            ),

            // Piece S
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { false, true, true }, { true, true, false } }),
                    new Shape(new bool[,] { { true, false }, { true, true }, { false, true } })
                ]
            ),

            // Piece T
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { false, true, false }, { true, true, true } }),
                    new Shape(new bool[,] { { true, false }, { true, true }, { true, false } }),
                    new Shape(new bool[,] { { true, true, true }, { false, true, false } }),
                    new Shape(new bool[,] { { false, true }, { true, true }, { false, true } })
                ]
            ),

            // Piece Z
            new PieceTemplate
            (
                [
                    new Shape(new bool[,] { { true, true, false }, { false, true, true } }),
                    new Shape(new bool[,] { { false, true }, { true, true }, { true, false } })
                ]
            )
        ];
    }
}