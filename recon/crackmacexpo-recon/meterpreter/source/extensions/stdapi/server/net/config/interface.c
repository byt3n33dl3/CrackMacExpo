#include "precomp.h"
#include "common_metapi.h"

#include <iptypes.h>
#include <ws2ipdef.h>

DWORD get_interfaces_mib(Remote *remote, Packet *response)
{
	DWORD tableSize = sizeof(MIB_IPADDRROW);
	DWORD index;
	MIB_IFROW iface;

	PMIB_IPADDRTABLE table = (PMIB_IPADDRTABLE)malloc(sizeof(PMIB_IPADDRTABLE));
	if (table == NULL)
	{
		return ERROR_OUTOFMEMORY;
	}

	// attempt with an insufficient buffer size
	DWORD result = GetIpAddrTable(table, &tableSize, TRUE);
	if (result == ERROR_INSUFFICIENT_BUFFER)
	{
		table = (PMIB_IPADDRTABLE)realloc(table, tableSize);

		if (table == NULL)
		{
			return ERROR_OUTOFMEMORY;
		}

		if (GetIpAddrTable(table, &tableSize, TRUE) != NO_ERROR)
		{
			free(table);
			return GetLastError();
		}
	}
	// it might have worked with a single row!
	else if (result != NO_ERROR)
	{
		free(table);
		return GetLastError();
	}

	// Enumerate the entries
	for (index = 0; index < table->dwNumEntries; index++)
	{
		Packet* group = met_api->packet.create_group();

		met_api->packet.add_tlv_uint(group, TLV_TYPE_INTERFACE_INDEX, table->table[index].dwIndex);
		met_api->packet.add_tlv_raw(group, TLV_TYPE_IP, (PUCHAR)&table->table[index].dwAddr, sizeof(DWORD));
		met_api->packet.add_tlv_raw(group, TLV_TYPE_NETMASK, (PUCHAR)&table->table[index].dwMask, sizeof(DWORD));

		iface.dwIndex = table->table[index].dwIndex;

		// If interface information can get gotten, use it.
		if (GetIfEntry(&iface) == NO_ERROR)
		{
			met_api->packet.add_tlv_raw(group, TLV_TYPE_MAC_ADDR, (PUCHAR)iface.bPhysAddr, iface.dwPhysAddrLen);
			met_api->packet.add_tlv_uint(group, TLV_TYPE_INTERFACE_MTU, iface.dwMtu);

			if (iface.bDescr)
			{
				met_api->packet.add_tlv_string(group, TLV_TYPE_MAC_NAME, iface.bDescr);
			}
		}

		// Add the interface group
		met_api->packet.add_group(response, TLV_TYPE_NETWORK_INTERFACE, group);
	}

	free(table);
	return ERROR_SUCCESS;
}

DWORD get_interfaces(Remote *remote, Packet *response)
{
	DWORD result = ERROR_SUCCESS;

	ULONG flags = GAA_FLAG_INCLUDE_PREFIX
		| GAA_FLAG_SKIP_DNS_SERVER
		| GAA_FLAG_SKIP_MULTICAST
		| GAA_FLAG_SKIP_ANYCAST;

	LPSOCKADDR sockaddr;

	ULONG family = AF_UNSPEC;
	IP_ADAPTER_ADDRESSES *pAdapters = NULL;
	IP_ADAPTER_ADDRESSES *pCurr = NULL;
	ULONG outBufLen = 0;
	DWORD(WINAPI *gaa)(DWORD, DWORD, void *, void *, void *);

	// Use the newer version so we're guaranteed to have a large enough struct.
	// Unfortunately, using these probably means it won't compile on older
	// versions of Visual Studio.  =(
	IP_ADAPTER_UNICAST_ADDRESS_LH *pAddr = NULL;
	IP_ADAPTER_UNICAST_ADDRESS_LH *pPref = NULL;
	// IP_ADAPTER_PREFIX is only defined if NTDDI_VERSION > NTDDI_WINXP
	// Since we request older versions of things, we have to be explicit
	// when using newer structs.
	IP_ADAPTER_PREFIX_XP *pPrefix = NULL;

	// We can't rely on the `Length` parameter of the IP_ADAPTER_PREFIX_XP struct
	// to tell us if we're on Vista or not because it always comes out at 48 bytes
	// so we have to check the version manually.
	OSVERSIONINFOEX v;

	gaa = (DWORD(WINAPI *)(DWORD, DWORD, void*, void*, void*))GetProcAddress(
		GetModuleHandle("iphlpapi"), "GetAdaptersAddresses");
	if (!gaa)
	{
		dprintf("[INTERFACE] No 'GetAdaptersAddresses'. Falling back on get_interfaces_mib");
		return get_interfaces_mib(remote, response);
	}

	gaa(family, flags, NULL, pAdapters, &outBufLen);
	if (!(pAdapters = malloc(outBufLen)))
	{
		return ERROR_NOT_ENOUGH_MEMORY;
	}

	if (gaa(family, flags, NULL, pAdapters, &outBufLen))
	{
		result = GetLastError();
		goto out;
	}

	dprintf("[INTERFACE] pAdapters->Length = %d", pAdapters->Length);
	// According to http://msdn.microsoft.com/en-us/library/windows/desktop/aa366058(v=vs.85).aspx
	// the PIP_ADAPTER_PREFIX doesn't exist prior to XP SP1. We check for this via the `Length`
	// value, which is 72 in XP without an SP, but 144 in later versions.
	if (pAdapters->Length <= 72)
	{
		dprintf("[INTERFACE] PIP_ADAPTER_PREFIX is missing");
		result = get_interfaces_mib(remote, response);
		goto out;
	}

	// we'll need to know the version later on
	memset(&v, 0, sizeof(v));
	v.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	GetVersionEx((LPOSVERSIONINFO)&v);

	// Enumerate the entries
	for (pCurr = pAdapters; pCurr; pCurr = pCurr->Next)
	{
		// Save the first prefix for later in case we don't have an OnLinkPrefixLength
		pPrefix = pCurr->FirstPrefix;

		Packet* group = met_api->packet.create_group();

		dprintf("[INTERFACE] Adding index: %u", pCurr->IfIndex);
		met_api->packet.add_tlv_uint(group, TLV_TYPE_INTERFACE_INDEX, pCurr->IfIndex);

		dprintf("[INTERFACE] Adding MAC");
		met_api->packet.add_tlv_raw(group, TLV_TYPE_MAC_ADDR, (PUCHAR)pCurr->PhysicalAddress, pCurr->PhysicalAddressLength);

		dprintf("[INTERFACE] Adding Description");
		met_api->packet.add_tlv_wstring(group, TLV_TYPE_MAC_NAME, pCurr->Description);

		dprintf("[INTERFACE] Adding MTU: %u", pCurr->Mtu);
		met_api->packet.add_tlv_uint(group, TLV_TYPE_INTERFACE_MTU, pCurr->Mtu);

		for (pAddr = (IP_ADAPTER_UNICAST_ADDRESS_LH*)pCurr->FirstUnicastAddress;
			pAddr; pAddr = pAddr->Next)
		{
			sockaddr = pAddr->Address.lpSockaddr;
			if (AF_INET != sockaddr->sa_family && AF_INET6 != sockaddr->sa_family)
			{
				// Skip interfaces that aren't IP
				continue;
			}

			DWORD prefix = 0;
			if (v.dwMajorVersion >= 6) {
				// Then this is Vista+ and the OnLinkPrefixLength member
				// will be populated
				dprintf("[INTERFACES] >= Vista, using prefix: %x", pAddr->OnLinkPrefixLength);
				prefix = htonl(pAddr->OnLinkPrefixLength);
			}
			else if (pPrefix)
			{
				dprintf("[INTERFACES] < Vista, using prefix: %x", pPrefix->PrefixLength);
				prefix = htonl(pPrefix->PrefixLength);
			}
			else
			{
				dprintf("[INTERFACES] < Vista, no prefix");
				prefix = 0;
			}

			if (prefix)
			{
				dprintf("[INTERFACE] Adding Prefix: %x", prefix);
				// the UINT value is already byte-swapped, so we add it as a raw instead of
				// swizzling the bytes twice.
				met_api->packet.add_tlv_raw(group, TLV_TYPE_IP_PREFIX, (PUCHAR)&prefix, sizeof(prefix));
			}

			if (sockaddr->sa_family == AF_INET)
			{
				dprintf("[INTERFACE] Adding IPv4 Address: %x", ((struct sockaddr_in *)sockaddr)->sin_addr);
				met_api->packet.add_tlv_raw(group, TLV_TYPE_IP, (PUCHAR)&(((struct sockaddr_in *)sockaddr)->sin_addr), 4);
			}
			else
			{
				dprintf("[INTERFACE] Adding IPv6 Address");
				met_api->packet.add_tlv_raw(group, TLV_TYPE_IP, (PUCHAR)&(((struct sockaddr_in6 *)sockaddr)->sin6_addr), 16);
				met_api->packet.add_tlv_raw(group, TLV_TYPE_IP6_SCOPE, (PUCHAR)&(((struct sockaddr_in6 *)sockaddr)->sin6_scope_id), sizeof(DWORD));
			}

		}
		// Add the interface group
		met_api->packet.add_group(response, TLV_TYPE_NETWORK_INTERFACE, group);
	}

out:
	free(pAdapters);

	return result;
}

/*
 * Returns zero or more local interfaces to the requestor
 */
DWORD request_net_config_get_interfaces(Remote *remote, Packet *packet)
{
	Packet *response = met_api->packet.create_response(packet);
	DWORD result = ERROR_SUCCESS;

	result = get_interfaces(remote, response);

	// Transmit the response if valid
	met_api->packet.transmit_response(result, remote, response);

	return result;
}




