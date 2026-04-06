import { createClient, type SupabaseClient } from '@supabase/supabase-js';
import { PUBLIC_SUPABASE_PUBLISHABLE_KEY, PUBLIC_SUPABASE_URL } from '$env/static/public';

let browserClient: SupabaseClient | null = null;

export function getSupabaseBrowserClient(): SupabaseClient {
	if (!PUBLIC_SUPABASE_URL || !PUBLIC_SUPABASE_PUBLISHABLE_KEY) {
		throw new Error('Supabase env vars are missing. Check web/.env.local.');
	}

	if (!browserClient) {
		browserClient = createClient(PUBLIC_SUPABASE_URL, PUBLIC_SUPABASE_PUBLISHABLE_KEY, {
			auth: {
				persistSession: true,
				autoRefreshToken: true,
				detectSessionInUrl: true
			}
		});
	}

	return browserClient;
}