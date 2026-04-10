import { createClient, type SupabaseClient } from '@supabase/supabase-js';
import { PUBLIC_SUPABASE_URL, PUBLIC_SUPABASE_PUBLISHABLE_KEY } from '$env/static/public';

let browserClient: SupabaseClient | null = null;

export class SupabaseConfigError extends Error {
	constructor(message = 'Supabase public env vars were missing when this build was created.') {
		super(message);
		this.name = 'SupabaseConfigError';
	}
}

export function getSupabaseBrowserClient(): SupabaseClient {
	const supabaseUrl = PUBLIC_SUPABASE_URL;
	const supabasePublishableKey = PUBLIC_SUPABASE_PUBLISHABLE_KEY;

	if (!supabaseUrl || !supabasePublishableKey) {
		throw new SupabaseConfigError(
			'Supabase public env vars were missing when this site was built. Re-run the web deploy after updating the GitHub Actions secrets.'
		);
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