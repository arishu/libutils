// libutils.h
#ifndef LIBUTILS_H_INCLUDED
#define LIBUTILS_H_INCLUDED

#ifdef LIBUTILS_EXPORTS
	#define LIBUTILS_API __declspec(dllexport)
#else
	#define LIBUTILD_API __declspec(dllimport)
#endif // LIBUTILS_EXPORTS

#endif // !LIBUTILS_H_INCLUDED

