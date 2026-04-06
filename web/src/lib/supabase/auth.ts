import type { RealtimeChannel } from '@supabase/supabase-js';
import { getSupabaseBrowserClient } from './client';

export function initializeSupabaseAuth(): () => void {
	const supabase = getSupabaseBrowserClient();
	const {
		data: { subscription }
	} = supabase.auth.onAuthStateChange(() => {
		// Keep the singleton warm so browser-side session persistence and refresh stay active.
	});

	void supabase.auth.getSession();

	return () => {
		subscription.unsubscribe();
	};
}

export function subscribeToTable(
	channelName: string,
	table: string,
	onChange: () => void
): { channel: RealtimeChannel; cleanup: () => void } {
	const supabase = getSupabaseBrowserClient();
	const channel = supabase
		.channel(channelName)
		.on('postgres_changes', { event: '*', schema: 'public', table }, onChange)
		.subscribe();

	return {
		channel,
		cleanup: () => {
			void supabase.removeChannel(channel);
		}
	};
}