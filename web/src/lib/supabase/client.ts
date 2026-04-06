import { createClient, type SupabaseClient } from '@supabase/supabase-js';
import { env } from '$env/dynamic/public';

let browserClient: SupabaseClient | null = null;

export function getSupabaseBrowserClient(): SupabaseClient {
	const supabaseUrl = env.PUBLIC_SUPABASE_URL;
	const supabasePublishableKey = env.PUBLIC_SUPABASE_PUBLISHABLE_KEY;

	if (!supabaseUrl || !supabasePublishableKey) {
		throw new Error('Supabase env vars are missing. Check web/.env.local.');
	}

	if (!browserClient) {
		browserClient = createClient(supabaseUrl, supabasePublishableKey, {
			auth: {
				persistSession: true,
				autoRefreshToken: true,
				detectSessionInUrl: true
			}
		});
	}

	return browserClient;
}