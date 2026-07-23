#include "MainApi.h"

void* CreateLine(double x1, double y1, double x2, double y2)
{
	LineWrapper* wrapper = new LineWrapper();
	wrapper->Line = new Geom2d_Line(gp_Pnt2d(x1, y1), gp_Dir2d(x2 - x1, y2 - y1));
	return wrapper;
}
