#pragma once

#include "Base.h"

#include <Geom2d_Line.hxx>

class LineWrapper
{
public:
	Handle(Geom2d_Line) Line;
};

/// <summary>
/// 创建直线
/// </summary>
dll_export void* CreateLine(double x1, double y1, double x2, double y2);