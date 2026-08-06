#pragma once

#include "Base.h"

#include <Geom2d_Line.hxx>
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
};

/// <summary>
/// 表示无限延伸的直线
/// </summary>
class LineWrapper : public CurveWrapper
{
public:
	Handle(Geom2d_Line) Line;
	Handle(Geom2d_Curve) GetCurve() override { return Line; }
};

/// <summary>
/// 表示直线段
/// </summary>
class LineSegmentWrapper : public CurveWrapper
{
public:
	Handle(Geom2d_TrimmedCurve) LineSegment;
	Handle(Geom2d_Curve) GetCurve() override { return LineSegment; }
};

/// <summary>
/// 初始化二维点缓存
/// </summary>
dll_export void InitPoint2DCache(double* cache, int size);

/// <summary>
/// 创建直线
/// </summary>
dll_export void* CreateLine(double x1, double y1, double x2, double y2);

/// <summary>
/// 创建直线段
/// </summary>
dll_export void* CreateLineSegment(double x1, double y1, double x2, double y2);

/// <summary>
/// 获取两条曲线的交点
/// </summary>
dll_export int GetIntersection(CurveWrapper* curve1, CurveWrapper* curve2);