#ifndef _METERPRETER_SOURCE_EXTENSION_PRIV_PRIV_SERVER_ELEVATE_SERVICE_H
#define _METERPRETER_SOURCE_EXTENSION_PRIV_PRIV_SERVER_ELEVATE_SERVICE_H

DWORD service_start( char * cpName );

DWORD service_stop( char * cpName );

DWORD service_create( char * cpName, char * cpPath );

DWORD service_destroy( char * cpName );

DWORD service_query_status( char * cpName, DWORD* dwState );

DWORD service_wait_for_status( char * cpName, DWORD dwStatus, DWORD dwMaxTimeout );

#endif
