#pragma once

#include "Base.h"

#include <Geom2d_Line.hxx>
#include <Geom2d_Circle.hxx>
#include <GCE2d_MakeSegment.hxx>

/// <summary>
/// 表示平面上的曲线
/// </summary>
class CurveWrapper
{
public:
	virtual ~CurveWrapper() = default;

public:
	virtual Handle(Geom2d_Curve) GetCurve() { return nullptr; }
	virtual void FreeCurve() {}
};

/// <summary>
/// 表示无限延伸的直线
/// </summary>
class LineWrapper : public CurveWrapper
{
public:
	Handle(Geom2d_Line) Line;
	Handle(Geom2d_Curve) GetCurve() override { return Line; }
	void FreeCurve() override { Line.Nullify(); }
};

/// <summary>
/// 表示直线段
/// </summary>
class LineSegmentWrapper : public CurveWrapper
{
public:
	Handle(Geom2d_TrimmedCurve) LineSegment;
	Handle(Geom2d_Curve) GetCurve() override { return LineSegment; }
	void FreeCurve() override { LineSegment.Nullify(); }
};

/// <summary>
/// 表示圆形
/// </summary>
class CircleWrapper : public CurveWrapper
{
public:
	Handle(Geom2d_Circle) Circle;
	Handle(Geom2d_Curve) GetCurve() override { return Circle; }
	void FreeCurve() override { Circle.Nullify(); }
};
